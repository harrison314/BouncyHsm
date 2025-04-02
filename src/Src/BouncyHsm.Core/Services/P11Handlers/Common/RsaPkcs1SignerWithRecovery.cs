using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class RsaPkcs1SignerWithRecovery : ISignerWithRecovery
{
    private readonly IAsymmetricBlockCipher rsaEngine;
    private readonly IDigest signingDigest;
    private RsaDigestSigner? rsaDigestSigner;
    private Pkcs1Encoding? verifier;
    private byte[]? recoveredMessage;

    public string AlgorithmName
    {
        get => "RSA/PKCS1.5";
    }

    public RsaPkcs1SignerWithRecovery(IAsymmetricBlockCipher rsaEngine)
    {
        this.rsaEngine = rsaEngine;
        this.signingDigest = new NullDigest();
    }

    public RsaPkcs1SignerWithRecovery(IAsymmetricBlockCipher rsaEngine, IDigest signingDigest)
    {
        this.rsaEngine = rsaEngine;
        this.signingDigest = signingDigest;
    }

    public void Init(bool forSigning, ICipherParameters parameters)
    {
        if (forSigning)
        {
            this.signingDigest.Reset();
            this.rsaDigestSigner = new RsaDigestSigner(this.rsaEngine, this.signingDigest, (AlgorithmIdentifier?)null);
            this.rsaDigestSigner.Init(true, parameters);
        }
        else
        {
            this.verifier = new Pkcs1Encoding(this.rsaEngine);
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
        if (this.rsaDigestSigner == null)
        {
            throw new InvalidOperationException("RsaPkcs1SignerWithRecovery: BlockUpdate is enabled only for signing.");
        }

        this.rsaDigestSigner.BlockUpdate(input);
    }

    public byte[] GenerateSignature()
    {
        if (this.rsaDigestSigner == null)
        {
            throw new InvalidOperationException("RsaPkcs1SignerWithRecovery not initialized for signature generation.");
        }

        return this.rsaDigestSigner.GenerateSignature();
    }

    public int GetMaxSignatureSize()
    {
        if (this.rsaDigestSigner == null)
        {
            throw new InvalidOperationException("RsaPkcs1SignerWithRecovery not initialized for signature generation.");
        }

        return this.rsaDigestSigner.GetMaxSignatureSize();
    }

    public byte[] GetRecoveredMessage()
    {
        return this.recoveredMessage!;
    }

    public bool HasFullMessage()
    {
        throw new NotSupportedException("Method HasFullMessage is not supported in RsaPkcs1SignerWithRecovery.");
    }

    public void Reset()
    {
        this.recoveredMessage = null;
        this.rsaDigestSigner?.Reset();
    }

    public void Update(byte input)
    {
        if (this.rsaDigestSigner == null)
        {
            throw new InvalidOperationException("RsaPkcs1SignerWithRecovery: BlockUpdate is enabled only for signing.");
        }

        this.rsaDigestSigner.Update(input);
    }

    public void UpdateWithRecoveredMessage(byte[] signature)
    {
        throw new NotSupportedException("Method UpdateWithRecoveredMessage is not supported in RsaPkcs1SignerWithRecovery.");
    }

    public bool VerifySignature(byte[] signature)
    {
        if (this.verifier == null)
        {
            throw new InvalidOperationException("RsaPkcs1SignerWithRecovery not initialized for verification");
        }
        this.recoveredMessage = null;
        try
        {
            this.recoveredMessage = this.verifier.ProcessBlock(signature, 0, signature.Length);

            //TODO: Chech recoveredMessage is Pkcs1 Digest Info?
            return true;
        }
        catch (InvalidCipherTextException)
        {
            return false;
        }
    }
}
