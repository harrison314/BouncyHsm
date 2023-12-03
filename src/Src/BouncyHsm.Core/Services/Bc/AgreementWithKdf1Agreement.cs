using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;

namespace BouncyHsm.Core.Services.Bc;

//https://cdn.preterhuman.net/texts/cryptography/Key%20Agreement%20and%20Key%20Transport%20Using%20Elliptic%20Curve%20Cryptography.pdf
// section 5.6.3 Key Derivation Functions for CKM_ECDH1_DERIVE
internal class AgreementWithKdf1Agreement : IBasicAgreement
{
    private readonly IBasicAgreement agreement;
    private readonly int keySize;
    private readonly IDigest kdfDigest;
    private readonly byte[]? sharedData;

    public AgreementWithKdf1Agreement(IBasicAgreement agreement,int keySize, IDigest kdfDigest, byte[]? sharedData)
    {
        System.Diagnostics.Debug.Assert(this.kdfDigest is not NullDigest);
        this.agreement = agreement;
        this.keySize = keySize;
        this.kdfDigest = kdfDigest;
        this.sharedData = sharedData;
    }

    public void Init(ICipherParameters parameters)
    {
        this.agreement.Init(parameters);
    }

    public BigInteger CalculateAgreement(ICipherParameters pubKey)
    {
        BigInteger result = this.agreement.CalculateAgreement(pubKey);

        byte[] z = result.ToByteArrayUnsigned();
        byte[] keyData = new byte[this.keySize];

        Kdf1BytesGenerator kdfGenerator = new Kdf1BytesGenerator(this.kdfDigest);
        kdfGenerator.Init(new KdfParameters(z, this.sharedData));
        kdfGenerator.GenerateBytes(keyData);

        return new BigInteger(1, keyData);
    }

    public int GetFieldSize()
    {
        return this.agreement.GetFieldSize();
    }
}