using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BouncyHsm.Infrastructure.Common;

internal static class StringOperations
{
    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static bool FixedTimeEquals(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        bool result = true;
        for (int i = 0; i < a.Length; i++)
        {
            result &= a[i] == b[i];
        }

        return result;
    }
}
