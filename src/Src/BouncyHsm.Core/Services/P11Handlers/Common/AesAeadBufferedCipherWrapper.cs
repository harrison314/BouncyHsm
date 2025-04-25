using BouncyHsm.Core.Services.Bc;
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
        this.bufferedCipher.Init(true, this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_ENCRYPT, keyObject));

        return this.bufferedCipher;
    }

    public IBufferedCipher IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject);
        this.bufferedCipher.Init(false, this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_DECRYPT, keyObject));

        return this.bufferedCipher;
    }

    public IWrapper IntoWrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoWrapping with object id {objectId}.", keyObject);
        BufferedCipherWrapper wrapper = new BufferedCipherWrapper(this.bufferedCipher, false);
        wrapper.Init(true, this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_WRAP, keyObject));

        return wrapper;
    }

    public IWrapper IntoUnwrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoUnwrapping with object id {objectId}.", keyObject);

        BufferedCipherWrapper wrapper = new BufferedCipherWrapper(this.bufferedCipher, false);
        wrapper.Init(false, this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_UNWRAP, keyObject));

        return wrapper;
    }

    private ICipherParameters CreateCipherParams(BufferedCipherWrapperOperation operation, KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to CreateCipherParams.");

        if (keyObject is AesKeyObject aesKeyObject)
        {
            bool opEnable = operation switch
            {
                BufferedCipherWrapperOperation.CKA_ENCRYPT => aesKeyObject.CkaEncrypt,
                BufferedCipherWrapperOperation.CKA_DECRYPT => aesKeyObject.CkaDecrypt,
                BufferedCipherWrapperOperation.CKA_WRAP => aesKeyObject.CkaWrap,
                BufferedCipherWrapperOperation.CKA_UNWRAP => aesKeyObject.CkaUnwrap,
                _ => throw new InvalidProgramException($"Enum value {operation} is not supported.")
            };

            if (!opEnable)
            {
                this.logger.LogError("Object with id {ObjectId} can not set {operation} to true.", keyObject.Id, operation);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    $"The operation is not allowed because objet is not authorized to encrypt ({operation} must by true).");
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
