using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Modes;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class ChaCha20Poly1305CipherWrapper : ICipherWrapper
{
    private readonly byte[] nonce;
    private readonly byte[]? aadData;
    private readonly CKM mechanismType;
    private readonly ILogger<ChaCha20Poly1305CipherWrapper> logger;

    public ChaCha20Poly1305CipherWrapper(byte[] nonce, byte[]? aadData, CKM mechanismType, ILogger<ChaCha20Poly1305CipherWrapper> logger)
    {
        this.nonce = nonce;
        this.aadData = aadData;
        this.mechanismType = mechanismType;
        this.logger = logger;
    }

    public CipherUinion IntoDecryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoDecryption with object id {objectId}.", keyObject.Id);

        ChaCha20Poly1305 engine = new ChaCha20Poly1305();
        engine.Init(false, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_DECRYPT, keyObject), this.nonce));
        if (this.aadData != null)
        {
            engine.ProcessAadBytes(this.aadData.AsSpan());
        }

        return new CipherUinion.AeadCipher(engine);
    }

    public CipherUinion IntoEncryption(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoEncryption with object id {objectId}.", keyObject.Id);

        ChaCha20Poly1305 engine = new ChaCha20Poly1305();
        engine.Init(true, new ParametersWithIV(this.CreateCipherParams(BufferedCipherWrapperOperation.CKA_ENCRYPT, keyObject), this.nonce));
        if (this.aadData != null)
        {
            engine.ProcessAadBytes(this.aadData.AsSpan());
        }

        return new CipherUinion.AeadCipher(engine);
    }

    public IWrapper IntoUnwrapping(KeyObject keyObject)
    {
        throw new NotSupportedException("In ChaCha20Poly1305CipherWrapper is not supported unwraping.");
    }

    public IWrapper IntoWrapping(KeyObject keyObject)
    {
        throw new NotSupportedException("In ChaCha20Poly1305CipherWrapper is not supported wraping.");
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
