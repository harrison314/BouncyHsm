namespace BouncyHsm.Core.Services.Contracts;

public record GetSlotSpecification(bool WithTokenPresent) : ISpecification;
