using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;

namespace BouncyHsm.Core.Services.Bc;

internal class HmacGeneralSignerAdapter : ISigner
{
    private readonly HMac hmac;
    private readonly int generalParameter;
    private bool? forSigning;

    public string AlgorithmName
    {
        get => this.hmac.AlgorithmName;
    }

    public HmacGeneralSignerAdapter(HMac hmac, int generalParameter)
    {
        if (generalParameter < 0) throw new ArgumentOutOfRangeException("Parameter can not by less than zero.", nameof(generalParameter));
        if (generalParameter > hmac.GetMacSize()) throw new ArgumentOutOfRangeException($"Parameter can not by more than HMAC size ({hmac.GetMacSize()}).", nameof(generalParameter));

        this.hmac = hmac;
        this.generalParameter = generalParameter;
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

        int hmacSize = this.hmac.GetMacSize();
        Span<byte> buffer = (hmacSize > 512) ? new byte[hmacSize] : stackalloc byte[hmacSize];
        this.hmac.DoFinal(buffer);

        if (this.generalParameter == 0)
        {
            return Array.Empty<byte>();
        }

        return buffer.Slice(0, this.generalParameter).ToArray();
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

        if (this.generalParameter == 0)
        {
            return signature.Length == 0;
        }

        return buffer.Slice(0, this.generalParameter).SequenceEqual(signature);
    }
}