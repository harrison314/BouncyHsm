namespace BouncyHsm.Core;

public static class RuntimeSwitches
{
    public static bool SkipAttributeChecks
    {
        get;
    }

    public static bool SkipExplicitUnwrapPadding
    {
        get;
    }

    public static int BadHsmLevel
    {
        get;
    }

    static RuntimeSwitches()
    {
        BadHsmLevel = 0;
        SkipAttributeChecks = false;
        SkipExplicitUnwrapPadding = false;

        string? skipAttributeChecks = Environment.GetEnvironmentVariable("BOUNCYHSM_BADHSM_LEVEL");

        if (string.Equals(skipAttributeChecks, "1", StringComparison.Ordinal))
        {
            BadHsmLevel = 1;
            SkipAttributeChecks = true;
            SkipExplicitUnwrapPadding = false;
        }
        else if (string.Equals(skipAttributeChecks, "2", StringComparison.Ordinal))
        {
            BadHsmLevel = 2;
            SkipAttributeChecks = true;
            SkipExplicitUnwrapPadding = true;
        }
    }
}
