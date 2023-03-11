namespace BouncyHsm.DocGenerator;

public record MechanismInfo(string MechanismType, 
    uint MinKeySize, 
    uint MaxKeySize,
    ParsedMechanismFlags Flags);
