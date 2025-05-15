using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class EnableByVersionProfileOperation : ProfileOperation
{
    public Pkcs11SpecVersion Pkcs11SpecVersion
    {
        get;
        set;
    }

    public EnableByVersionProfileOperation()
    {

    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        foreach (KeyValuePair<CKM, MechanismInfo> mechanism in originalMechanisms)
        {
            if (mechanism.Value.SpecVersion == this.Pkcs11SpecVersion)
            {
                mechanisms[mechanism.Key] = mechanism.Value;
            }
        }
    }
}
