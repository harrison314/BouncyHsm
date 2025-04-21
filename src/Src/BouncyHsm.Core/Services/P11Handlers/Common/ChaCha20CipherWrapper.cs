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

internal class ChaCha20CipherWrapper : ICipherWrapper
{
    public const int Chacha20NonceSize = 8;
    public const int Chacha20_7539NonceSize = 12;

    private readonly byte[] nonce;
    private readonly CKM mechanismType;
    private readonly ILogger<ChaCha20CipherWrapper> logger;

    public ChaCha20CipherWrapper(byte[] nonce, CKM mechanismType, ILogger<ChaCha20CipherWrapper> logger)
    {
        this.nonce = nonce;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public CipherUinion IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject.Id);

        IStreamCipher engine = this.CreateChacha20();
        engine.Init(false, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_DECRYPT, keyObject), this.nonce));

        return new CipherUinion.StreamCipher(engine);
    }

    public CipherUinion IntoEncryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoEncryption with object id {objectId}.", keyObject.Id);

        IStreamCipher engine = this.CreateChacha20();
        engine.Init(true, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_ENCRYPT, keyObject), this.nonce));

        return new CipherUinion.StreamCipher(engine);
    }

    public IWrapper IntoUnwrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoUnwrapping with object id {objectId}.", keyObject.Id);
        PlainStreamCipherWrapper wrapper = new PlainStreamCipherWrapper(this.CreateChacha20());
        wrapper.Init(false, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_UNWRAP, keyObject), this.nonce));

        return wrapper;
    }

    public IWrapper IntoWrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoWrapping with object id {objectId}.", keyObject.Id);

        PlainStreamCipherWrapper wrapper = new PlainStreamCipherWrapper(this.CreateChacha20());
        wrapper.Init(true, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_WRAP, keyObject), this.nonce));

        return wrapper;
    }

    private IStreamCipher CreateChacha20()
    {
        this.logger.LogTrace("Create Chacha20 engine with nonce size {nonceSize}.", this.nonce.Length);

        return this.nonce.Length switch
        {
            Chacha20NonceSize => new ChaChaEngine(20),
            Chacha20_7539NonceSize => new ChaCha7539Engine(),
            _ => throw new InvalidOperationException($"Invalid nonce size {this.nonce.Length}B.")
        };
    }

    private ICipherParameters CreateCipherParams(BufferedCipherWrapperOperation operation, KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to CreateCipherParams.");

        if (keyObject is ChaCha20KeyObject chacha20KeyObject)
        {
            bool opEnable = operation switch
            {
                BufferedCipherWrapperOperation.CKA_WRAP => chacha20KeyObject.CkaWrap,
                BufferedCipherWrapperOperation.CKA_UNWRAP => chacha20KeyObject.CkaUnwrap,
                BufferedCipherWrapperOperation.CKA_ENCRYPT => chacha20KeyObject.CkaEncrypt,
                BufferedCipherWrapperOperation.CKA_DECRYPT => chacha20KeyObject.CkaDecrypt,
                _ => throw new InvalidProgramException($"Enum value {operation} is not supported.")
            };

            if (!opEnable)
            {
                this.logger.LogError("Object with id {ObjectId} can not set {operation} to true.", keyObject.Id, operation);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    $"The operation is not allowed because objet is not authorized to encrypt ({operation} must by true).");
            }

            return new KeyParameter(chacha20KeyObject.GetSecret());
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required AES key.");
        }
    }
}
