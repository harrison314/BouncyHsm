namespace BouncyHsm.Core.Services.Contracts;

public record PersistentRepositoryStats(int SlotCount, int TotalObjectCount, int PrivateKeys, int X509Certificates);