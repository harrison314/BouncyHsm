using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using System.Collections.ObjectModel;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class WrapKeyHandler : IRpcRequestHandler<WrapKeyRequest, WrapKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<WrapKeyHandler> logger;

    public WrapKeyHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<WrapKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async Task<WrapKeyEnvelope> Handle(WrapKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        KeyObject wrappingKey = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.WrappingKeyHandle, cancellationToken);
        KeyObject key = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_WRAP);
        wrappingKey.CheckAllowedMechanism((CKM)request.Mechanism.MechanismType, this.logger);

        this.CheckExtractable(key);
        this.CheckKeyByWrapTemplate(wrappingKey, key);
        this.CheckWrapWithTrusted(wrappingKey, key);

        BufferedCipherWrapperFactory cipherFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        ICipherWrapper cipherWrapper = cipherFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IWrapper wrapper = cipherWrapper.IntoWrapping(wrappingKey);

        byte[] keyData = this.EncodeKey(key);
        this.logger.LogTrace("Encode key has length {Length}.", keyData.Length);
        byte[] wrappedKey = wrapper.Wrap(keyData, 0, keyData.Length);
        this.logger.LogTrace("Wrapped key has length {Length}.", keyData.Length);

        if (request.IsPtrWrappedKeySet)
        {
            if (request.PulWrappedKeyLen < wrappedKey.Length)
            {
                throw new RpcPkcs11Exception(CKR.CKR_BUFFER_TOO_SMALL, $"Wrapped data buffer is small ({request.PulWrappedKeyLen}, required is {wrappedKey.Length}).");
            }

            return new WrapKeyEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new WrapKeyData()
                {
                    PulWrappedKeyLen = (uint)wrappedKey.Length,
                    WrappedKeyData = wrappedKey
                }
            };
        }
        else
        {
            return new WrapKeyEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new WrapKeyData()
                {
                    PulWrappedKeyLen = (uint)wrappedKey.Length,
                    WrappedKeyData = Array.Empty<byte>()
                }
            };
        }
    }

    private void CheckKeyByWrapTemplate(KeyObject wrappingKey, KeyObject key)
    {
        this.logger.LogTrace("Entering to CheckKeyByWrapTemplate");

        IReadOnlyDictionary<CKA, IAttributeValue> wrappingTemplate = wrappingKey switch
        {
            PublicKeyObject publicKeyObject => publicKeyObject.CkaWrapTemplate,
            SecretKeyObject secretKeyObject => secretKeyObject.CkaWrapTemplate,
            _ => ReadOnlyDictionary<CKA, IAttributeValue>.Empty,
        };

        if (wrappingTemplate.Count == 0)
        {
            return;
        }

        if (!key.IsMatch(wrappingTemplate))
        {
            this.logger.LogError("The wrapping key {WrappingKey} can not wrap the key {Key} because the key does not match the requirements in the CKA_WRAP_TEMPLATE template.", wrappingKey, key);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID,
                $"The wrapping key {wrappingKey} can not wrap the key {key} because the key does not match the requirements in the CKA_WRAP_TEMPLATE template.");
        }
    }

    private void CheckExtractable(KeyObject key)
    {
        bool extractable = key switch
        {
            SecretKeyObject secretKeyObject => secretKeyObject.CkaExtractable,
            PrivateKeyObject privateKeyObject => privateKeyObject.CkaExtractable,
            _ => throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Key handle of wrapping key is invalid.")
        };

        if (!extractable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_NOT_WRAPPABLE, $"Key {key} can not enable CKA_EXTRACTABLE.");
        }

        if (key is SecretKeyObject genericSecret && genericSecret.CkaKeyType == CKK.CKK_GENERIC_SECRET)
        {
            this.logger.LogWarning("Secret key object CKO_SECRET_KEY of type CKK_GENERIC_SECRET many HSMs do not allow unwrap.");
        }
    }

    private void CheckWrapWithTrusted(KeyObject wrappingKey, KeyObject key)
    {
        this.logger.LogTrace("Entering to CheckKeyByWrapTemplate");

        bool keyRequiresTrustedWrapper = key switch
        {
            SecretKeyObject sk => sk.CkaWrapWithTrusted,
            PrivateKeyObject pk => pk.CkaWrapWithTrusted,
            _ => false
        };

        if (keyRequiresTrustedWrapper)
        {
            bool wrappingKeyIsTrusted = wrappingKey switch
            {
                SecretKeyObject sk => sk.CkaTrusted,
                PublicKeyObject pk => pk.CkaTrusted,
                _ => false
            };

            if (!wrappingKeyIsTrusted)
            {
                this.logger.LogError("The wrapping key {WrappingKey} must have CKA_TRUSTED set to true, because the key {Key} it wraps has CKA_WRAP_WITH_TRUSTED set to true.",
                    wrappingKey,
                    key);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID,
                    $"The wrapping key {wrappingKey} must have CKA_TRUSTED set to true, because the key {key} it wraps has CKA_WRAP_WITH_TRUSTED set to true.");
            }
        }
    }

    private byte[] EncodeKey(KeyObject key)
    {
        this.logger.LogTrace("Entering to EncodeKey witj key {key}.", key);

        if (key is SecretKeyObject secretKeyObject)
        {
            return secretKeyObject.GetSecret();
        }

        if (key is EcdsaPrivateKeyObject ecdsaPrivateKeyObject)
        {
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter privateKeyParams = ecdsaPrivateKeyObject.GetPrivateKey();
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParams);
            PrivateKeyInfo newKeyInfo = this.ReEncodeEcPrivateKeyInfo(info);

            return newKeyInfo.GetEncoded();
        }

        if (key is PrivateKeyObject privateKeyObject)
        {
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter privateKeyParams = privateKeyObject.GetPrivateKey();
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParams);
            return info.GetEncoded();
        }

        throw new RpcPkcs11Exception(CKR.CKR_KEY_NOT_WRAPPABLE,
            $"Can not wrap object {key}.");
    }

    private PrivateKeyInfo ReEncodeEcPrivateKeyInfo(PrivateKeyInfo standardBcPrivateKey)
    {
        ECPrivateKeyStructure originalKey = ECPrivateKeyStructure.GetInstance(standardBcPrivateKey.ParsePrivateKey());
        BigInteger privateKeyValue = new BigInteger(1, originalKey.PrivateKey.GetOctets());

        ECPrivateKeyStructure newKeyStructure = new ECPrivateKeyStructure(privateKeyValue.BitLength,
            privateKeyValue,
            originalKey.PublicKey, // Public key is optional by PKCS11 standard capther 6.7
            null);

        PrivateKeyInfo newKeyInfo = new PrivateKeyInfo(standardBcPrivateKey.PrivateKeyAlgorithm,
            newKeyStructure,
            standardBcPrivateKey.Attributes);

        return newKeyInfo;
    }
}
