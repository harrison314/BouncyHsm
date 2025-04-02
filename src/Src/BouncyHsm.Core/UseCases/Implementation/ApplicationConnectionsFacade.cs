using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class ApplicationConnectionsFacade: IApplicationConnectionsFacade
{
    private readonly IClientApplicationContext clientAppCtx;
    private readonly ILogger<ApplicationConnectionsFacade> logger;

    public ApplicationConnectionsFacade(IClientApplicationContext clientAppCtx, ILogger<ApplicationConnectionsFacade> logger)
    {
        this.clientAppCtx = clientAppCtx;
        this.logger = logger;
    }

    public ValueTask<DomainResult<SlotConnections>> GetApplicationConnections(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlotApplicationConnections.");

        List<ApplicationSession> sessions = this.clientAppCtx.GetActiveMemorySessions()
            .Select(t => new ApplicationSession(t.Id,
                t.Data.ComputerName,
                t.Data.ApplicationName,
                Convert.ToUInt32(t.Data.Pid),
                t.StartAt,
                t.LastActivity))
            .ToList();

        DomainResult<SlotConnections> result = new DomainResult<SlotConnections>.Ok(new SlotConnections(sessions));
        return new ValueTask<DomainResult<SlotConnections>>(result);
    }

    public ValueTask<VoidDomainResult> RemoveApplicationConnection(Guid applicationSessionId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to RemoveApplicationConnection with applicationSessionId {applicationSessionId}.", applicationSessionId);

        try
        {
            this.clientAppCtx.ReleaseMemorySession(applicationSessionId);
            this.logger.LogInformation("Removed application session/connection with applicationSessionId {applicationSessionId}.", applicationSessionId);

            return new ValueTask<VoidDomainResult>(new VoidDomainResult.Ok());
        }
        catch (BouncyHsmNotFoundException ex)
        {
            this.logger.LogError(ex, "Error in RemoveApplicationConnection applicationSessionId {applicationSessionId} not found in application sessions.", applicationSessionId);

            return new ValueTask<VoidDomainResult>(new VoidDomainResult.NotFound());
        }
    }
}
