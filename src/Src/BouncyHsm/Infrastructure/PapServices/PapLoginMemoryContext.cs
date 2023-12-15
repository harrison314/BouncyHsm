using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Services.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace BouncyHsm.Infrastructure.PapServices;

public class PapLoginMemoryContext : IPapLoginMemoryContext
{
    private readonly IHubContext<PapHub, IPapClient> hubContext;
    private readonly IOptions<BouncyHsmSetup> bouncyHsmSetup;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]?>> loginSessions;
    private readonly ILogger<PapLoginMemoryContext> logger;

    public PapLoginMemoryContext(IHubContext<PapHub, IPapClient> hubContext,
        IOptions<BouncyHsmSetup> bouncyHsmSetup,
        ILogger<PapLoginMemoryContext> logger)
    {
        this.hubContext = hubContext;
        this.bouncyHsmSetup = bouncyHsmSetup;
        this.logger = logger;
        this.loginSessions = new ConcurrentDictionary<string, TaskCompletionSource<byte[]?>>();
    }

    public async Task<byte[]?> PerformLogin(string loginType, string tokenInfo, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Enering to PerformLogin.");

        string logSession = this.CreateSessionId();
        this.logger.LogTrace("Create a new logSession {logSession}.", logSession);
        await this.hubContext.Clients.All.NotifyLoginInit(new LoginInitData(logSession,
              loginType,
              tokenInfo),
              cancellationToken);

        try
        {
            TaskCompletionSource<byte[]?> resultTaskSrc = new TaskCompletionSource<byte[]?>();
            this.loginSessions.TryAdd(logSession, resultTaskSrc);

            return await resultTaskSrc.Task.WaitAsync(this.bouncyHsmSetup.Value.ProtectedAuthPathTimeout, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            this.logger.LogError(ex, "Log session {logSession} was cancelled.", logSession);
            return null;
        }
        finally
        {
            this.loginSessions.TryRemove(logSession, out _);
            this.logger.LogDebug("Log session {logSession} removed.", logSession);
            await this.hubContext.Clients.All.NotifyLoginCancel(logSession, cancellationToken);
            this.logger.LogDebug("Send NotifyLoginCancel for log session {logSession}.", logSession);
        }
    }

    public void CancellLogin(string logSession)
    {
        this.logger.LogTrace("Entering to CancellLogin with logSession {logSession}.", logSession);
        if (this.loginSessions.TryGetValue(logSession, out TaskCompletionSource<byte[]?>? result))
        {
            result.SetResult(null);
            this.logger.LogDebug("LogSession {logSession} was cancelled.", logSession);
        }
        else
        {
            this.logger.LogError("Log session {logSession} not found for protected authorization path.", logSession);
            throw new BouncyHsmException($"Log session {logSession} not found for protected authorization path.");
        }
    }

    public void Login(string logSession, byte[] login)
    {
        this.logger.LogTrace("Entering to Login with logSession {logSession}.", logSession);

        if (this.loginSessions.TryGetValue(logSession, out TaskCompletionSource<byte[]?>? result))
        {
            result.SetResult(login);
            this.logger.LogDebug("LogSession {logSession} has set as login.", logSession);
        }
        else
        {
            this.logger.LogError("Log session {logSession} not found for protected authorization path.", logSession);
            throw new BouncyHsmException($"Log session {logSession} not found for protected authorization path.");
        }
    }

    private string CreateSessionId()
    {
        return Guid.NewGuid().ToString("d");
    }
}
