using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using MessagePack;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DigestInitHandler : IRpcRequestHandler<DigestInitRequest, DigestInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<DigestInitHandler> logger;

    public DigestInitHandler(IP11HwServices hwServices, ILogger<DigestInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DigestInitEnvelope> Handle(DigestInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}, {digestType}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        p11Session.State.EnsureEmpty();

        IDigest bcDigest = this.CreateDigest(request.Mechanism);
        p11Session.State = new DigestSessionState(bcDigest);

        return new DigestInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    private IDigest CreateDigest(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateDigest with mechanism type {mechanismType}", mechanism.MechanismType);

        MechanismUtils.CheckMechanism(mechanism, MechanismCkf.CKF_DIGEST);
        CKM ckMechanism = (CKM)mechanism.MechanismType;

        IDigest bdDigest = ckMechanism switch
        {
            CKM.CKM_MD2 => new MD2Digest(),
            CKM.CKM_MD5 => new MD5Digest(),
            CKM.CKM_SHA_1 => new Sha1Digest(),
            CKM.CKM_SHA224 => new Sha224Digest(),
            CKM.CKM_SHA256 => new Sha256Digest(),
            CKM.CKM_SHA384 => new Sha384Digest(),
            CKM.CKM_SHA512 => new Sha512Digest(),
            CKM.CKM_SHA512_256 => new Sha512tDigest(256),
            CKM.CKM_SHA512_224 => new Sha512tDigest(224),
            CKM.CKM_SHA512_T => this.BuildSha512T(mechanism),

            CKM.CKM_SHA3_256 => new Sha3Digest(256),
            CKM.CKM_SHA3_224 => new Sha3Digest(224),
            CKM.CKM_SHA3_384 => new Sha3Digest(384),
            CKM.CKM_SHA3_512 => new Sha3Digest(512),

            CKM.CKM_RIPEMD128 => new RipeMD128Digest(),
            CKM.CKM_RIPEMD160 => new RipeMD160Digest(),

            CKM.CKM_GOSTR3411 => new Gost3411Digest(),
            CKM.CKM_FASTHASH => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Not support digest algorithm {ckMechanism}."),

            CKM.CKM_BLAKE2B_160 => new Blake2bDigest(160),
            CKM.CKM_BLAKE2B_256 => new Blake2bDigest(256),
            CKM.CKM_BLAKE2B_384 => new Blake2bDigest(384),
            CKM.CKM_BLAKE2B_512 => new Blake2bDigest(512),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for digesting.")
        };

        return bdDigest;
    }

    private IDigest BuildSha512T(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to BuildSha512T.");

        try
        {
            CkP_MacGeneralParams generalParams = MessagePack.MessagePackSerializer.Deserialize<CkP_MacGeneralParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
            return new Sha512tDigest(Convert.ToInt32(generalParams.Value));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds Sha512T digest from parameter.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for digest mechanism CKM_SHA512_T.", ex);
        }
    }
}
