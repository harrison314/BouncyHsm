using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class Salsa20CipherWrapper : ICipherWrapper
{
    public const int Salsa20NonceSize = 8;
    public const int XSalsa20NonceSize = 24;

    private readonly byte[] nonce;
    private readonly CKM mechanismType;
    private readonly ILogger<Salsa20CipherWrapper> logger;

    public Salsa20CipherWrapper(byte[] nonce, CKM mechanismType, ILogger<Salsa20CipherWrapper> logger)
    {
        this.nonce = nonce;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public CipherUinion IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject.Id);

        IStreamCipher engine = this.CreateSalsa20();
        engine.Init(false, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_DECRYPT, keyObject), this.nonce));

        return new CipherUinion.StreamCipher(engine);
    }

    public CipherUinion IntoEncryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoEncryption with object id {objectId}.", keyObject.Id);

        IStreamCipher engine = this.CreateSalsa20();
        engine.Init(true, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_ENCRYPT, keyObject), this.nonce));

        return new CipherUinion.StreamCipher(engine);
    }

    public IWrapper IntoUnwrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoUnwrapping with object id {objectId}.", keyObject.Id);
        PlainStreamCipherWrapper wrapper = new PlainStreamCipherWrapper(this.CreateSalsa20());
        wrapper.Init(false, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_UNWRAP, keyObject), this.nonce));

        return wrapper;
    }

    public IWrapper IntoWrapping(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoWrapping with object id {objectId}.", keyObject.Id);

        PlainStreamCipherWrapper wrapper = new PlainStreamCipherWrapper(this.CreateSalsa20());
        wrapper.Init(true, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_WRAP, keyObject), this.nonce));

        return wrapper;
    }

    private IStreamCipher CreateSalsa20()
    {
        this.logger.LogTrace("Create Salsa20 engine with nonce size {nonceSize}.", this.nonce.Length);

        return this.nonce.Length switch
        {
            Salsa20NonceSize => new Salsa20Engine(20),
            XSalsa20NonceSize => new XSalsa20Engine(),
            _ => throw new InvalidOperationException($"Invalid nonce size {this.nonce.Length}B.")
        };
    }

    private ICipherParameters CreateCipherParams(BufferedCipherWrapperOperation operation, KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to CreateCipherParams.");

        if (keyObject is Salsa20KeyObject salsa20KeyObject)
        {
            bool opEnable = operation switch
            {
                BufferedCipherWrapperOperation.CKA_WRAP => salsa20KeyObject.CkaWrap,
                BufferedCipherWrapperOperation.CKA_UNWRAP => salsa20KeyObject.CkaUnwrap,
                BufferedCipherWrapperOperation.CKA_ENCRYPT => salsa20KeyObject.CkaEncrypt,
                BufferedCipherWrapperOperation.CKA_DECRYPT => salsa20KeyObject.CkaDecrypt,
                _ => throw new InvalidProgramException($"Enum value {operation} is not supported.")
            };

            if (!opEnable)
            {
                this.logger.LogError("Object with id {ObjectId} can not set {operation} to true.", keyObject.Id, operation);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    $"The operation is not allowed because objet is not authorized to encrypt ({operation} must by true).");
            }

            return new KeyParameter(salsa20KeyObject.GetSecret());
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required AES key.");
        }
    }
}