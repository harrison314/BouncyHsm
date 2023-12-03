using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class Ecdh1CofactorDeriveKeyGenerator : Ecdh1DeriveKeyGenerator
{
    private readonly ILogger logger;
    public Ecdh1CofactorDeriveKeyGenerator(Ecdh1DeriveParams ecdh1Params, ILogger<Ecdh1CofactorDeriveKeyGenerator> logger)
        : base(ecdh1Params, logger)
    {
        this.logger = logger;
    }

    protected override IBasicAgreement CreateAgreement(CKD kdfFunction, int minKeySize, byte[]? sharedData)
    {
        this.logger.LogTrace("Entering to CreateAgreement with KDF {Kdf}, minKeySize {minKeySize}, contains sharedData {containsSharedData}.",
            kdfFunction,
            minKeySize,
            sharedData != null);

        return kdfFunction switch
        {
            CKD.CKD_NULL => new ECDHCBasicAgreement(),
            CKD.CKD_SHA1_KDF => new AgreementWithKdf1Agreement(new ECDHCBasicAgreement(), minKeySize, new Sha1Digest(), sharedData),
            CKD.CKD_SHA224_KDF => new AgreementWithKdf1Agreement(new ECDHCBasicAgreement(), minKeySize, new Sha224Digest(), sharedData),
            CKD.CKD_SHA256_KDF => new AgreementWithKdf1Agreement(new ECDHCBasicAgreement(), minKeySize, new Sha256Digest(), sharedData),
            CKD.CKD_SHA384_KDF => new AgreementWithKdf1Agreement(new ECDHCBasicAgreement(), minKeySize, new Sha384Digest(), sharedData),
            CKD.CKD_SHA512_KDF => new AgreementWithKdf1Agreement(new ECDHCBasicAgreement(), minKeySize, new Sha512Digest(), sharedData),
            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"kdf {kdfFunction} from CK_ECDH1_DERIVE_PARAMS is not supported or invalid.")
        };
    }

    public override string ToString()
    {
        return "Ecdh1CofactorDeriveKeyGenerator";
    }
}