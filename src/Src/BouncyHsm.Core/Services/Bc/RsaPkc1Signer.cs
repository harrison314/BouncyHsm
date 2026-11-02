using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Bc;

// Copy and modify from BouncyCastle
internal class RsaPkc1Signer : ISigner
{
    private readonly IAsymmetricBlockCipher m_engine;
    private readonly IDigest m_digest;
    private bool m_forSigning;

    public string AlgorithmName
    {
        get => "RSA";
    }

    public RsaPkc1Signer()
    {
        this.m_engine = new Pkcs1Encoding(new RsaBlindedEngine(new RsaCoreEngine()));
        this.m_digest = new NullDigest();
    }

    public RsaPkc1Signer(IDigest digest)
    {
        this.m_engine = new Pkcs1Encoding(new RsaBlindedEngine(new RsaCoreEngine()));
        this.m_digest = digest;
    }

    public void Init(bool forSigning, ICipherParameters parameters)
    {
        this.m_forSigning = forSigning;

        AsymmetricKeyParameter key = (AsymmetricKeyParameter)ParameterUtilities.IgnoreRandom(parameters);

        if (forSigning && !key.IsPrivate)
            throw new InvalidKeyException("Signing requires private key.");

        if (!forSigning && key.IsPrivate)
            throw new InvalidKeyException("Verification requires public key.");

        this.Reset();

        this.m_engine.Init(forSigning, parameters);
    }

    public void Update(byte input)
    {
        this.m_digest.Update(input);
    }

    public void BlockUpdate(byte[] input, int inOff, int inLen)
    {
        this.m_digest.BlockUpdate(input, inOff, inLen);
    }

    public void BlockUpdate(ReadOnlySpan<byte> input)
    {
        this.m_digest.BlockUpdate(input);
    }

    public byte[] GenerateSignature()
    {
        if (!this.m_forSigning)
            throw new InvalidOperationException("RsaDigestSigner not initialised for signature generation.");

        byte[] data = DigestUtilities.DoFinal(this.m_digest);

        try
        {
            return this.m_engine.ProcessBlock(data, 0, data.Length);
        }
        catch (Exception e) when (!(e is CryptoException))
        {
            throw new CryptoException("unable to encode signature: " + e.Message, e);
        }
    }

    public int GetMaxSignatureSize()
    {
        return this.m_engine.GetOutputBlockSize();
    }

    public void Reset()
    {
        this.m_digest.Reset();
    }

    public bool VerifySignature(byte[] signature)
    {
        if (this.m_forSigning)
            throw new InvalidOperationException("RsaDigestSigner not initialised for verification");

        byte[] sig;
        try
        {
            sig = this.m_engine.ProcessBlock(signature, 0, signature.Length);
        }
        catch (Exception)
        {
            return false;
        }

        byte[] data = DigestUtilities.DoFinal(this.m_digest);

        return Arrays.FixedTimeEquals(sig, data);
    }
}
