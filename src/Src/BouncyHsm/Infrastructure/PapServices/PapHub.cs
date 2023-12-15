using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace BouncyHsm.Infrastructure.PapServices;

public class PapHub : Hub<IPapClient>
{
    private readonly IPapLoginMemoryContext papLoginMemoryContext;
    private readonly ILogger<PapHub> logger;

    public PapHub(IPapLoginMemoryContext papLoginMemoryContext, ILogger<PapHub> logger)
    {
        this.papLoginMemoryContext = papLoginMemoryContext;
        this.logger = logger;
    }

    public Task SetLogin(string logSession, string loginValue)
    {
        this.logger.LogTrace("Entering to SetLogin with logSession {logSession}", logSession);
        this.papLoginMemoryContext.Login(logSession, Encoding.UTF8.GetBytes(loginValue));

        return Task.CompletedTask;
    }

    public Task CancellLogin(string logSession)
    {
        this.logger.LogTrace("Entering to CancellLogin with logSession {logSession}.", logSession);
        this.papLoginMemoryContext.CancellLogin(logSession);

        return Task.CompletedTask;
    }
}
