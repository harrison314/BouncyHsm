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
        bool result = a.Length == b.Length;
        for (int i = 0; i < a.Length; i++)
        {
            result &= a[i] == b[i % b.Length];
        }

        return result;
    }
}
