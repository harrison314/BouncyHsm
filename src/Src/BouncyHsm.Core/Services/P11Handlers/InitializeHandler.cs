using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class InitializeHandler : IRpcRequestHandler<InitializeRequest, InitializeEnvelope>
{
    private readonly IClientApplicationContext clientApplicationContext;
    private readonly ILogger<InitializeHandler> logger;

    public InitializeHandler(IClientApplicationContext clientApplicationContext, ILogger<InitializeHandler> logger)
    {
        this.clientApplicationContext = clientApplicationContext;
        this.logger = logger;
    }

    public ValueTask<InitializeEnvelope> Handle(InitializeRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req IsMutexFnSet {IsMutexFnSet}, LibraryCantCreateOsThreads {LibraryCantCreateOsThreads}, OsLockingOk {OsLockingOk}.",
            request.IsMutexFnSet,
            request.LibraryCantCreateOsThreads,
            request.OsLockingOk);


        string key = DataTransform.GetApplicationKey(request.AppId);
        this.clientApplicationContext.RegisterMemorySession(key);

        this.logger.LogInformation("Initialized client with nonce: {nonce} machine: {machine} pid: {pid}, CK_ULONG size {ckUlongSize}b, pointer size {pointerSize}b, platform: {Platform}, client version {clientVersion}.",
            request.AppId.AppNonce,
            request.ClientInfo.CompiuterName,
            request.AppId.Pid,
            request.ClientInfo.CkUlongSize * 8,
            request.ClientInfo.PointerSize * 8,
            request.ClientInfo.Platform,
            request.ClientInfo.LibVersion);

        InitializeEnvelope envelope = new InitializeEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };

        return new ValueTask<InitializeEnvelope>(envelope);
    }
}
