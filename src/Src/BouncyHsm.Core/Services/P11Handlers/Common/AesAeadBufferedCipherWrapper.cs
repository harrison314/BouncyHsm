using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class AesAeadBufferedCipherWrapper : IBufferedCipherWrapper
{
    private readonly IBufferedCipher bufferedCipher;
    private readonly int macSize;
    private readonly byte[]? nonce;
    private readonly byte[]? associatedText;
    private readonly CKM mechanismType;
    private readonly ILogger<AesAeadBufferedCipherWrapper> logger;

    public AesAeadBufferedCipherWrapper(IBufferedCipher bufferedCipher,
        int macSize,
        byte[]? nonce,
        byte[]? associatedText,
        CKM mechanismType,
        ILogger<AesAeadBufferedCipherWrapper> logger)
    {
        this.bufferedCipher = bufferedCipher;
        this.macSize = macSize;
        this.nonce = nonce;
        this.associatedText = associatedText;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public IBufferedCipher IntoEncryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoEncryption with object id {objectId}.", keyObject);
        this.bufferedCipher.Init(true, this.CreateChiperParams(true, keyObject));

        return this.bufferedCipher;
    }

    public IBufferedCipher IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject);
        this.bufferedCipher.Init(false, this.CreateChiperParams(false, keyObject));

        return this.bufferedCipher;
    }

    private ICipherParameters CreateChiperParams(bool foreEncrypt, KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to CreateChiperParams.");

        if (keyObject is AesKeyObject aesKeyObject)
        {
            if (foreEncrypt)
            {
                if (!aesKeyObject.CkaEncrypt)
                {
                    this.logger.LogError("Object with id {ObjectId} can not set CKA_ENCRYPT to true.", keyObject.Id);
                    throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                        "The encryption operation is not allowed because objet is not authorized to encrypt (CKA_ENCRYPT must by true).");
                }
            }
            else
            {
                if (!aesKeyObject.CkaDecrypt)
                {
                    this.logger.LogError("Object with id {ObjectId} can not set CKA_DECRYPT to true.", keyObject.Id);
                    throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                        "The decryption operation is not allowed because objet is not authorized to decrypt (CKA_DECRYPT must by true).");
                }
            }

            return new AeadParameters(new KeyParameter(aesKeyObject.GetSecret()),
                this.macSize,
                this.nonce,
                this.associatedText);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required AES key.");
        }
    }
}
