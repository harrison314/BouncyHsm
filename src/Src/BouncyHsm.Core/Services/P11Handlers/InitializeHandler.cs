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
    private readonly static string? CoreLibVersion = typeof(InitializeHandler).Assembly.GetName().Version?.ToString();

    public InitializeHandler(IClientApplicationContext clientApplicationContext, ILogger<InitializeHandler> logger)
    {
        this.clientApplicationContext = clientApplicationContext;
        this.logger = logger;
    }

    public Task<InitializeEnvelope> Handle(InitializeRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req IsMutexFnSet {IsMutexFnSet}, LibraryCantCreateOsThreads {LibraryCantCreateOsThreads}, OsLockingOk {OsLockingOk}.",
            request.IsMutexFnSet,
            request.LibraryCantCreateOsThreads,
            request.OsLockingOk);

        MemorySessionData sessionData = new MemorySessionData(request.ClientInfo.CompiuterName,
            request.AppId.AppName,
            request.AppId.Pid,
            request.ClientInfo.PointerSize,
            request.ClientInfo.CkUlongSize);
        string key = DataTransform.GetApplicationKey(request.AppId);
        this.clientApplicationContext.RegisterMemorySession(key, sessionData);

        this.logger.LogInformation("Initialized client with nonce: {nonce} machine: {machine} pid: {pid}, CK_ULONG size {ckUlongSize}b, pointer size {pointerSize}b, platform: {Platform}, client version {clientVersion}.",
            request.AppId.AppNonce,
            request.ClientInfo.CompiuterName,
            request.AppId.Pid,
            request.ClientInfo.CkUlongSize * 8,
            request.ClientInfo.PointerSize * 8,
            request.ClientInfo.Platform,
            request.ClientInfo.LibVersion);

        this.CheckLibralyVersion(request.ClientInfo.LibVersion);

        InitializeEnvelope envelope = new InitializeEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };

        return Task.FromResult(envelope);
    }

    private void CheckLibralyVersion(string libVersion)
    {
        System.Diagnostics.Debug.Assert(libVersion != null);
        System.Diagnostics.Debug.Assert(CoreLibVersion != null);

        if (!string.Equals(CoreLibVersion, libVersion, StringComparison.Ordinal))
        {
            this.logger.LogWarning("Native library BouncyHsm.Pkcs11Lib version {libVersion} does not match BouncyHsm program version {programVersion}.",
                libVersion,
                CoreLibVersion);
        }
    }
}
