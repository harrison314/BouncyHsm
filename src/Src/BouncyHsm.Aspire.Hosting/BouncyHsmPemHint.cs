namespace Aspire.Hosting;

[Flags]
public enum BouncyHsmPemHint
{
    None = 0,
    ForDerivation = 1,
    ForEncapsulation = 2,
    ForEncryption = 4,
    ForSigning = 8,
    ForWrap = 16
}