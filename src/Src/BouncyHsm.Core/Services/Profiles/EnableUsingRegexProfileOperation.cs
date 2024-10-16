using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System.Text.RegularExpressions;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class EnableUsingRegexProfileOperation : ProfileOperation
{
    public string RegexPattern
    {
        get;
        set;
    }

    public EnableUsingRegexProfileOperation()
    {
        this.RegexPattern = ".*";
    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        Regex regex = new Regex(this.RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        foreach ((CKM mechanism, MechanismInfo mechanismInfo) in originalMechanisms)
        {
            if (regex.IsMatch(mechanism.ToString()))
            {
                _ = mechanisms.TryAdd(mechanism, mechanismInfo);
            }
        }
    }
}