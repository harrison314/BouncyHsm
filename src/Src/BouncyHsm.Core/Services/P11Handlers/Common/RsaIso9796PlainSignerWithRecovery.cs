using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Signers;
using System.Security.Cryptography;

namespace BouncyHsm.Core.Services.P11Handlers.Common;
internal class RsaIso9796PlainSignerWithRecovery : ISignerWithRecovery
{
    private readonly IAsymmetricBlockCipher rsaEngine;
    private Iso9796Signer? signer;
    private ISO9796d1Encoding? verifier;
    private byte[]? recoveredMessage;

    public string AlgorithmName
    {
        get => "RSA/Iso9796Plain";
    }

    public RsaIso9796PlainSignerWithRecovery(IAsymmetricBlockCipher rsaEngine)
    {
        this.rsaEngine = rsaEngine;
    }

    public void Init(bool forSigning, ICipherParameters parameters)
    {
        if (forSigning)
        {
            this.signer = new Iso9796Signer(this.rsaEngine, new NullDigest());
            this.signer.Init(true, parameters);
        }
        else
        {
            this.verifier = new ISO9796d1Encoding(this.rsaEngine);
            this.verifier.Init(false, parameters);
        }

        this.Reset();
    }

    public void BlockUpdate(byte[] input, int inOff, int inLen)
    {
        this.BlockUpdate(input.AsSpan(inOff, inLen));
    }

    public void BlockUpdate(ReadOnlySpan<byte> input)
    {
        if (this.signer == null)
        {
            throw new InvalidOperationException("RsaIso9796PlainSignerWithRecovery: BlockUpdate is enabled only for signing.");
        }

        this.signer.BlockUpdate(input);
    }

    public byte[] GenerateSignature()
    {
        if (this.signer == null)
        {
            throw new InvalidOperationException("RsaIso9796PlainSignerWithRecovery not initialised for signature generation.");
        }

        return this.signer.GenerateSignature();
    }

    public int GetMaxSignatureSize()
    {
        if (this.signer == null)
        {
            throw new InvalidOperationException("RsaIso9796PlainSignerWithRecovery not initialised for signature generation.");
        }

        return this.signer.GetMaxSignatureSize();
    }

    public byte[] GetRecoveredMessage()
    {
        return this.recoveredMessage!;
    }

    public bool HasFullMessage()
    {
        throw new NotSupportedException("Method HasFullMessage is not supported in RsaIso9796PlainSignerWithRecovery.");
    }

    public void Reset()
    {
        this.recoveredMessage = null;
        this.signer?.Reset();
    }

    public void Update(byte input)
    {
        if (this.signer == null)
        {
            throw new InvalidOperationException("RsaIso9796PlainSignerWithRecovery: BlockUpdate is enabled only for signing.");
        }

        this.signer.Update(input);
    }

    public void UpdateWithRecoveredMessage(byte[] signature)
    {
        throw new NotSupportedException("Method UpdateWithRecoveredMessage is not supported in RsaIso9796PlainSignerWithRecovery.");
    }

    public bool VerifySignature(byte[] signature)
    {
        if (this.verifier == null)
        {
            throw new InvalidOperationException("RsaIso9796PlainSignerWithRecovery not initialised for verification");
        }
        this.recoveredMessage = null;
        try
        {
            this.recoveredMessage = this.verifier.ProcessBlock(signature, 0, signature.Length);
            return true;
        }
        catch (InvalidCipherTextException)
        {
            return false;
        }
    }
}
