using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;

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

    public async ValueTask<WrapKeyEnvelope> Handle(WrapKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        KeyObject wrappingKey = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.WrappingKeyHandle, cancellationToken);
        KeyObject key = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_WRAP);
        this.CheckExtractable(key);

        BufferedCipherWrapperFactory cipherFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        IBufferedCipherWrapper cipherWrapper = cipherFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IWrapper wrapper = cipherWrapper.IntoWrapping(wrappingKey);

        byte[] keyData = this.EncodeKey(key);
        byte[] wrappedKey = wrapper.Wrap(keyData, 0, keyData.Length);

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

    private void CheckExtractable(KeyObject key)
    {
        bool extractable = key switch
        {
            SecretKeyObject secretKeyObject => secretKeyObject.CkaExtractable,
            PrivateKeyObject privateKeyObject => privateKeyObject.CkaExtractable,
            _ => throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Key handle of wrapping key is invalid.")
        };

        if(!extractable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_NOT_WRAPPABLE, $"Key {key} can not enable CKA_EXTRACTABLE.");
        }
    }

    private byte[] EncodeKey(KeyObject key)
    {
        this.logger.LogTrace("Entering to EncodeKey witj key {key}.", key);

        if (key is SecretKeyObject secretKeyObject)
        {
            return secretKeyObject.GetSecret();
        }

        if (key is PrivateKeyObject privateKeyObject)
        {
            Org.BouncyCastle.Crypto.AsymmetricKeyParameter privateKeyParams = privateKeyObject.GetPrivateKey();
            //Org.BouncyCastle.Security.PrivateKeyFactory.CreateKey()
            PrivateKeyInfo info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParams);
            return info.ParsePrivateKey().GetEncoded();
        }

        throw new RpcPkcs11Exception(CKR.CKR_KEY_NOT_WRAPPABLE,
            $"Can not wrap object {key}.");
    }
}
