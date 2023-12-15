namespace BouncyHsm.Infrastructure.PapServices;

public interface IPapLoginMemoryContext
{
    Task<byte[]?> PerformLogin(string loginType, string tokenInfo, CancellationToken cancellationToken);

    void CancellLogin(string logSession);

    void Login(string logSession, byte[] login);
}
