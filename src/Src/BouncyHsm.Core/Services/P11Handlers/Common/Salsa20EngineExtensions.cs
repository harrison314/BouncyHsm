using Org.BouncyCastle.Crypto.Engines;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class Salsa20EngineExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "engineState")]
    private static extern ref uint[] GetEngineState(Salsa20Engine salsaEngine);

    public static void SetCounter(this Salsa20Engine salsaEngine, ulong counter)
    {
        ref uint[] engineState = ref GetEngineState(salsaEngine);

        System.Diagnostics.Debug.Assert(engineState.Length == 16);

        engineState[8] = (uint)(counter & 0xFFFFFFFF);
        engineState[9] = (uint)(counter >> 32);
    }

    public static ulong GetCounter(this Salsa20Engine salsaEngine)
    {
        ref uint[] engineState = ref GetEngineState(salsaEngine);

        System.Diagnostics.Debug.Assert(engineState.Length == 16);

        return ((ulong)engineState[9] << 32) | (ulong)engineState[8];
    }
}
