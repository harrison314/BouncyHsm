namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal record struct PrfDataContext(int Counter, byte[]? AlternativeIteration, int DkmLenghth, int BlockTotalLength);