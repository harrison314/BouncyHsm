using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands;

internal static class CkaIdParser
{
    public static byte[] Parse(string input)
    {
        if (input.StartsWith("UTF8:", StringComparison.OrdinalIgnoreCase))
        {
            return Encoding.UTF8.GetBytes(input[5..]);
        }

        if (input.StartsWith("HEX:", StringComparison.OrdinalIgnoreCase))
        {
            return HexConvertorSlim.FromHex(input.AsSpan(4));
        }

        if (input.StartsWith("BASE64:", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.FromBase64String(input[7..]);
        }

        throw new ArgumentException("CkaId must start  by 'UTF8:', 'HEX:' or 'BASE64:' prefix for select encoding.");
    }
}
