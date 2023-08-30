using BouncyHsm.Spa.Utils;

namespace BouncyHsm.Spa.Shared;

public enum BinaryForm
{
    Utf8,
    Hex,
    Base64
}

internal static class BinaryFormExtensions
{
    public static byte[] GetCkaId(this BinaryForm form, string value)
    {
        return form switch
        {
            BinaryForm.Utf8 => System.Text.Encoding.UTF8.GetBytes(value),
            BinaryForm.Base64 => Convert.FromBase64String(value),
            BinaryForm.Hex => HexConvertorSlim.FromHex(value),
            _ => throw new InvalidProgramException($"Enum value {form} is not supported.")
        };
    }
}