using Org.BouncyCastle.Crypto.Engines;
using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class ChaChaEngineExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "engineState")]
    private static extern ref uint[] GetEngineState(Salsa20Engine chachaEngine);

    public static void SetChaChaCounter(this ChaChaEngine salsaEngine, uint counterLower, uint counterUpper)
    {
        ref uint[] engineState = ref GetEngineState(salsaEngine);

        System.Diagnostics.Debug.Assert(engineState.Length == 16);

        engineState[12] = counterLower;
        engineState[13] = counterUpper;
    }

    public static (uint, uint) GetChaChaCounter(this ChaChaEngine chachaEngine)
    {
        ref uint[] engineState = ref GetEngineState(chachaEngine);

        System.Diagnostics.Debug.Assert(engineState.Length == 16);

        return (engineState[12], engineState[13]);
    }

    public static void SetChaCha7539Counter(this ChaCha7539Engine salsaEngine, uint counterLower)
    {
        ref uint[] engineState = ref GetEngineState(salsaEngine);

        System.Diagnostics.Debug.Assert(engineState.Length == 16);

        engineState[12] = counterLower;
    }

    public static uint GetChaCha7539Counter(this ChaCha7539Engine chachaEngine)
    {
        ref uint[] engineState = ref GetEngineState(chachaEngine);

        System.Diagnostics.Debug.Assert(engineState.Length == 16);

        return engineState[12];
    }
}