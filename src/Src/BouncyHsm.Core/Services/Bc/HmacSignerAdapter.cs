using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Bc;

internal class HmacSignerAdapter : ISigner
{
    private readonly IMac hmac;
    private bool? forSigning;

    public string AlgorithmName
    {
        get => this.hmac.AlgorithmName;
    }

    public HmacSignerAdapter(IMac hmac)
    {
        this.hmac = hmac;
        this.forSigning = null;
    }

    public void Init(bool forSigning, ICipherParameters parameters)
    {
        this.hmac.Init(parameters);
        this.forSigning = forSigning;
    }

    public void Update(byte input)
    {
        this.hmac.Update(input);
    }

    public void BlockUpdate(byte[] input, int inOff, int inLen)
    {
        this.hmac.BlockUpdate(input, inOff, inLen);
    }

    public void BlockUpdate(ReadOnlySpan<byte> input)
    {
        this.hmac.BlockUpdate(input);
    }

    public int GetMaxSignatureSize()
    {
        return this.hmac.GetMacSize();
    }

    public void Reset()
    {
        this.hmac.Reset();
        this.forSigning = null;
    }

    public byte[] GenerateSignature()
    {
        if (!this.forSigning.HasValue || !this.forSigning.Value)
        {
            throw new InvalidOperationException("This instance is not initialized for signing.");
        }

        byte[] hmac = new byte[this.hmac.GetMacSize()];
        this.hmac.DoFinal(hmac);

        return hmac;
    }

    public bool VerifySignature(byte[] signature)
    {
        if (!this.forSigning.HasValue || this.forSigning.Value)
        {
            throw new InvalidOperationException("This instance is not initialized for verify signature.");
        }

        int hmacSize = this.hmac.GetMacSize();
        Span<byte> buffer = (hmacSize > 512) ? new byte[hmacSize] : stackalloc byte[hmacSize];
        this.hmac.DoFinal(buffer);

        return buffer.SequenceEqual(signature);
    }
}
