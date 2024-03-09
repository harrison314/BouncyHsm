using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System.Text.RegularExpressions;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class RemoveUsingRegexProfileOperation : ProfileOperation
{
    public string RegexPattern
    {
        get;
        set;
    }

    public RemoveUsingRegexProfileOperation()
    {
        this.RegexPattern = ".*";
    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        List<CKM> deleteList = new List<CKM>();
        Regex regex = new Regex(this.RegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        foreach ((CKM mechanism, _) in mechanisms)
        {
            if (regex.IsMatch(mechanism.ToString()))
            {
                deleteList.Add(mechanism);
            }
        }

        foreach (CKM mechanism in deleteList)
        {
            mechanisms.Remove(mechanism);
        }
    }
}
