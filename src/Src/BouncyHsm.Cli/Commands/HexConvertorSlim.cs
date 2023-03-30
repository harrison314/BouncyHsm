namespace BouncyHsm.Cli.Commands;

internal static class HexConvertorSlim
{
    public static byte[] FromHex(ReadOnlySpan<char> hexString)
    {
        if (hexString.Length % 2 == 1)
        {
            throw new ArgumentException("A hexadecimal string must have an even number of characters.", nameof(hexString));
        }

        byte[] buffer = new byte[hexString.Length / 2];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = byte.Parse(hexString.Slice(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        }

        return buffer;
    }

    public static string ToHex(ReadOnlySpan<byte> data)
    {
        Span<char> buffer = (data.Length <= 126)
            ? stackalloc char[data.Length * 2]
            : new char[data.Length * 2];

        for (int i = 0; i < data.Length; i++)
        {
            data[i].TryFormat(buffer.Slice(i * 2, 2), out _, "X2");
        }

        return new string(buffer);
    }
}