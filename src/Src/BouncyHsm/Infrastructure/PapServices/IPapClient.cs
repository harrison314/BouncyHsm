namespace BouncyHsm.Infrastructure.PapServices;

public interface IPapClient
{
    Task NotifyLoginInit(LoginInitData data, CancellationToken cancellationToken);

    Task NotifyLoginCancel(string loginSession, CancellationToken cancellationToken);
}
