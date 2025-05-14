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
        return this.CreateAgreement(new ECDHCBasicAgreement(), kdfFunction, minKeySize, sharedData);
    }

    protected override IRawAgreement CreateAgreement(IRawAgreement basicAgreement, CKD kdfFunction, byte[]? sharedData)
    {
        this.logger.LogError("Mongomery keys is not supported in cofactor.");
        throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, "Mongomery keys is not supported in cofactor.");
    }

    public override string ToString()
    {
        return "Ecdh1CofactorDeriveKeyGenerator";
    }
}