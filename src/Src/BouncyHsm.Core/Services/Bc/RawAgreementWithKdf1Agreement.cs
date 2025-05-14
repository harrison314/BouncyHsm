using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;

namespace BouncyHsm.Core.Services.Bc;

internal class RawAgreementWithKdf1Agreement : IRawAgreement
{
    private readonly IRawAgreement agreement;
    private readonly IDigest kdfDigest;
    private readonly byte[]? sharedData;

    public int AgreementSize
    {
        get => int.MaxValue;
    }

    public RawAgreementWithKdf1Agreement(IRawAgreement agreement, IDigest kdfDigest, byte[]? sharedData)
    {
        System.Diagnostics.Debug.Assert(this.kdfDigest is not NullDigest);
        this.agreement = agreement;
        this.kdfDigest = kdfDigest;
        this.sharedData = sharedData;
    }

    public void Init(ICipherParameters parameters)
    {
        this.agreement.Init(parameters);
    }

    public void CalculateAgreement(ICipherParameters publicKey, byte[] buf, int off)
    {
        byte[] z = new byte[this.agreement.AgreementSize];
        this.agreement.CalculateAgreement(publicKey, z, 0);


        Kdf1BytesGenerator kdfGenerator = new Kdf1BytesGenerator(this.kdfDigest);
        kdfGenerator.Init(new KdfParameters(z, this.sharedData));
        kdfGenerator.GenerateBytes(buf.AsSpan(off));
    }

    public void CalculateAgreement(ICipherParameters publicKey, Span<byte> output)
    {
        byte[] z = new byte[this.agreement.AgreementSize];
        this.agreement.CalculateAgreement(publicKey, z.AsSpan());

        Kdf1BytesGenerator kdfGenerator = new Kdf1BytesGenerator(this.kdfDigest);
        kdfGenerator.Init(new KdfParameters(z, this.sharedData));
        kdfGenerator.GenerateBytes(output);
    }
}
