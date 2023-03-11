using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class AesBufferedCipherWrapper : IBufferedCipherWrapper
{
    private readonly IBufferedCipher bufferedCipher;
    private readonly byte[]? iv;
    private readonly CKM mechanismType;
    private readonly ILogger<AesBufferedCipherWrapper> logger;

    public AesBufferedCipherWrapper(IBufferedCipher bufferedCipher,
        byte[]? iv,
        CKM mechanismType,
        ILogger<AesBufferedCipherWrapper> logger)
    {
        this.bufferedCipher = bufferedCipher;
        this.iv = iv;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public IBufferedCipher IntoEncryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoEncryption with object id {objectId}.", keyObject);
        this.bufferedCipher.Init(true, this.CreateChiperParams(BufferedCipherWrapperOperation.CKA_ENCRYPT, keyObject));

        return this.bufferedCipher;
    }

    public IBufferedCipher IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject);
        this.bufferedCipher.Init(false, this.CreateChiperParams(BufferedCipherWrapperOperation.CKA_DECRYPT, keyObject));

        return this.bufferedCipher;
    }

    public IWrapper IntoWraping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoWraping with object id {objectId}.", keyObject);
        BufferedChiperWrapper wrapper = new BufferedChiperWrapper(this.bufferedCipher);
        wrapper.Init(true, this.CreateChiperParams(BufferedCipherWrapperOperation.CKA_WRAP, keyObject));

        return wrapper;
    }

    public IWrapper IntoUnwraping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoUnwraping with object id {objectId}.", keyObject);

        BufferedChiperWrapper wrapper = new BufferedChiperWrapper(this.bufferedCipher);
        wrapper.Init(false, this.CreateChiperParams(BufferedCipherWrapperOperation.CKA_UNWRAP, keyObject));

        return wrapper;
    }

    private ICipherParameters CreateChiperParams(BufferedCipherWrapperOperation operation, KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to CreateChiperParams.");

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

            if (this.iv == null)
            {
                return new KeyParameter(aesKeyObject.GetSecret());
            }
            else
            {
                return new ParametersWithIV(new KeyParameter(aesKeyObject.GetSecret()), this.iv);
            }
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required AES key.");
        }
    }
}
