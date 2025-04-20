using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class Rfc5649BufferedCipherWrapper : ICipherWrapper
{
    private readonly IBlockCipher bufferedCipher;
    private readonly CKM mechanismType;
    private readonly ILogger<Rfc5649BufferedCipherWrapper> logger;

    public Rfc5649BufferedCipherWrapper(IBlockCipher blockCipher,
        CKM mechanismType,
        ILogger<Rfc5649BufferedCipherWrapper> logger)
    {
        this.bufferedCipher = blockCipher;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public CipherUinion IntoDecryption(KeyObject keyObject)
    {
        throw new NotSupportedException("In Rfc5649BufferedCipherWrapper is not supported decryption.");
    }

    public CipherUinion IntoEncryption(KeyObject keyObject)
    {
        throw new NotSupportedException("In Rfc5649BufferedCipherWrapper is not supported encryption.");
    }

    public IWrapper IntoWrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoWrapping with object id {objectId}.", keyObject.Id);
        Rfc5649WrapEngine wrapper = new Rfc5649WrapEngine(this.bufferedCipher);
        wrapper.Init(true, this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_WRAP, keyObject));

        return wrapper;
    }

    public IWrapper IntoUnwrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoUnwrapping with object id {objectId}.", keyObject.Id);

        Rfc5649WrapEngine wrapper = new Rfc5649WrapEngine(this.bufferedCipher);
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

            return new KeyParameter(aesKeyObject.GetSecret());
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required AES key.");
        }
    }
}
