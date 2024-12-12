using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class Iso9796Signer : ISigner
{
    private readonly IAsymmetricBlockCipher rsaEngine;
    private readonly IDigest digest;
    private ISO9796d1Encoding? encoding;
    private bool forSigning;

    public string AlgorithmName
    {
        get => $"{this.digest.AlgorithmName}with{this.rsaEngine.AlgorithmName}/Iso9796";
    }

    public Iso9796Signer(IAsymmetricBlockCipher rsaEngine, IDigest digest)
    {
        this.rsaEngine = rsaEngine;
        this.digest = digest;
    }

    public void Init(bool forSigning, ICipherParameters parameters)
    {
        this.encoding = new ISO9796d1Encoding(this.rsaEngine);
        this.encoding.Init(forSigning, parameters);
        this.forSigning = true;
        this.digest.Reset();
    }

    public void BlockUpdate(byte[] input, int inOff, int inLen)
    {
        this.digest.BlockUpdate(input, inOff, inLen);
    }

    public void BlockUpdate(ReadOnlySpan<byte> input)
    {
        this.digest.BlockUpdate(input);
    }

    public void Update(byte input)
    {
        this.digest.Update(input);
    }

    public int GetMaxSignatureSize()
    {
        if (this.encoding == null)
        {
            throw new InvalidOperationException("Iso9796PlainSigner not initialised.");
        }

        return this.encoding.GetOutputBlockSize();
    }

    public void Reset()
    {
        this.digest.Reset();
    }

    public byte[] GenerateSignature()
    {
        if (this.encoding == null || !this.forSigning)
        {
            throw new InvalidOperationException("Iso9796PlainSigner not initialised for signature generation.");
        }

        byte[] message = new byte[this.digest.GetDigestSize()];
        this.digest.DoFinal(message);

        return this.encoding.ProcessBlock(message, 0, message.Length);
    }

    public bool VerifySignature(byte[] signature)
    {
        if (this.encoding == null || !this.forSigning)
        {
            throw new InvalidOperationException("Iso9796PlainSigner not initialised for verification.");
        }

        byte[] message = new byte[this.digest.GetDigestSize()];
        this.digest.DoFinal(message);

        byte[] verificationMessage = this.encoding.ProcessBlock(signature, 0, signature.Length);

        bool isValid = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(message, verificationMessage);
        System.Security.Cryptography.CryptographicOperations.ZeroMemory(verificationMessage);
        System.Security.Cryptography.CryptographicOperations.ZeroMemory(message);

        return isValid;
    }
}
