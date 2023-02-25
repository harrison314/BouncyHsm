namespace BouncyHsm.Core.Services.Bc;

/// <summary>
/// keyUsage ::= BIT STRING {
///   digitalSignature(0),
///   nonRepudiation(1),
///   keyEncipherment(2),
///   dataEncipherment(3),
///   keyAgreement(4),
///   keyCertSign(5),
///   cRLSign(6),
///   encipherOnly(7),
///   decipherOnly(8)
/// }
/// </summary>
internal static class KeyUsageBitsIndex
{
    public const int DigitalSignature = 0;
    public const int NonRepudiation = 1;
    public const int KeyEncipherment = 2;
    public const int DataEncipherment = 3;
    public const int KeyAgreement = 4;
    public const int KeyCertSign = 5;
    public const int CRLSign = 6;
    public const int EncipherOnly = 7;
    public const int DecipherOnly = 8;
}
