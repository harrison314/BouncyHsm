using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class WrapperSignWithRecoverFactory
{
    private readonly ILogger<WrapperSignWithRecoverFactory> logger;
    private readonly ILoggerFactory loggerFactory;

    public WrapperSignWithRecoverFactory(ILoggerFactory loggerFactory)
    {
        this.logger = loggerFactory.CreateLogger<WrapperSignWithRecoverFactory>(); ;
        this.loggerFactory = loggerFactory;
    }

    public IWrapperSignWithRecover CreateSignatureWithAlgorithm(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateSignatureAlgorithm with mechanism type {mechanismType}", mechanism.MechanismType);

        CKM ckMechanism = (CKM)mechanism.MechanismType;

        //CKM.CKM_RSA_9796 -> Iso9796d2Signer 
        //Iso9796d2PssSigner

        //return new Iso9796d2Signer(new RsaBlindedEngine(), new Sha1Digest(), true);
        // Iso9796d2PssSigner eng = new Iso9796d2PssSigner(rsa, new RipeMD160Digest(), 20, true);


        //
        return ckMechanism switch
        {
            CKM.CKM_RSA_9796 => new RsaWrapperSignWithRecover(new Iso9796d2Signer(new RsaBlindedEngine(), new NullDigest(), isImplicit: true), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSignWithRecover>()),
            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for signing or validation with recover.")
        };
    }
}
