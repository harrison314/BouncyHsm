namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal record struct PrfDataContext(int Counter, int DkmLenghth, int BlockCount);