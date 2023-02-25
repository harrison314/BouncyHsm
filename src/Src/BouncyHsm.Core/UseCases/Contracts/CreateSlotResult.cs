namespace BouncyHsm.Core.UseCases.Contracts;

public record CreateSlotResult(Guid Id, uint SlotId, string TokenSerialNumber);
