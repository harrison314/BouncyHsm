namespace BouncyHsm.Core;

public static class RuntimeSwitches
{
    public static bool SkipAttributeChecks
    {
        get;
    }

    static RuntimeSwitches()
    {
        SkipAttributeChecks = false;

        string? skipAttributeChecks = Environment.GetEnvironmentVariable("BOUNCYHSM_BADHSM_LEVEL");

        if (string.Equals(skipAttributeChecks, "1", StringComparison.Ordinal))
        {
            SkipAttributeChecks = true;
        }
    }
}
