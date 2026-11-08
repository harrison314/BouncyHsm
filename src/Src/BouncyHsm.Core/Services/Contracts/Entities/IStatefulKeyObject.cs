namespace BouncyHsm.Core.Services.Contracts.Entities;

/// <summary>
/// Marker for key objects whose value advances on every use (e.g. HSS/LMS, XMSS private keys).
/// Writers of such objects must hold <see cref="ObjectWriteLocks"/> and reload the object inside it,
/// otherwise a stale copy could be persisted and a one-time key reused.
/// </summary>
public interface IStatefulKeyObject
{
}
