namespace BouncyHsm.Core.Services.Contracts;

public record ClientApplicationContextStats(int ConnectedApplications, int RoSessionCount, int RwSessionCount);