using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class RsaBufferedCipherWrapper : IBufferedCipherWrapper
{
    private readonly IBufferedCipher bufferedCipher;
    private readonly CKM mechanismType;
    private readonly ILogger<RsaBufferedCipherWrapper> logger;

    public RsaBufferedCipherWrapper(IBufferedCipher bufferedCipher, CKM mechanismType, ILogger<RsaBufferedCipherWrapper> logger)
    {
        this.bufferedCipher = bufferedCipher;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public IBufferedCipher IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoEncryption with object id {objectId}.", keyObject);

        if (keyObject is RsaPrivateKeyObject rsaPrivateKeyObject)
        {
            if (!rsaPrivateKeyObject.CkaDecrypt)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_DECRYPT to true.", keyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The decryption operation is not allowed because objet is not authorized to decrypt (CKA_DECRYPT must by true).");
            }

            this.bufferedCipher.Init(false, rsaPrivateKeyObject.GetPrivateKey());

            return this.bufferedCipher;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required private RSA key.");
        }
    }

    public IBufferedCipher IntoEncryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject);

        if (keyObject is RsaPublicKeyObject rsaPublicKeyObject)
        {
            if (!rsaPublicKeyObject.CkaEncrypt)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_ENCRYPT to true.", keyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The encryption operation is not allowed because objet is not authorized to encrypt (CKA_ENCRYPT must by true).");
            }

            this.bufferedCipher.Init(true, rsaPublicKeyObject.GetPublicKey());

            return this.bufferedCipher;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required private RSA key.");
        }
    }

    public IWrapper IntoUnwrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoUnwrapping with object id {objectId}.", keyObject);

        if (keyObject is RsaPrivateKeyObject rsaPrivateKeyObject)
        {
            if (!rsaPrivateKeyObject.CkaUnwrap)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_UNWRAP to true.", keyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The operation is not allowed because objet is not authorized to decrypt (CKA_UNWRAP must by true).");
            }

            BufferedCipherWrapper wrapper = new BufferedCipherWrapper(this.bufferedCipher, false);
            wrapper.Init(false, rsaPrivateKeyObject.GetPrivateKey());

            return wrapper;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required private RSA key.");
        }
    }

    public IWrapper IntoWrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoWrapping with object id {objectId}.", keyObject);

        if (keyObject is RsaPublicKeyObject rsaPublicKeyObject)
        {
            if (!rsaPublicKeyObject.CkaWrap)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_WRAP to true.", keyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The operation is not allowed because objet is not authorized to encrypt (CKA_WRAP must by true).");
            }

            BufferedCipherWrapper wrapper = new BufferedCipherWrapper(this.bufferedCipher, false);
            wrapper.Init(true, rsaPublicKeyObject.GetPublicKey());

            return wrapper;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required private RSA key.");
        }
    }
}