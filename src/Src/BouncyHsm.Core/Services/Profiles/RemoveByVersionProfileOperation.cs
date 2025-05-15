using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class RemoveByVersionProfileOperation : ProfileOperation
{
    public Pkcs11SpecVersion Pkcs11SpecVersion
    {
        get;
        set;
    }

    public RemoveByVersionProfileOperation()
    {

    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        List<CKM> toRemove = new List<CKM>();
        foreach (KeyValuePair<CKM, MechanismInfo> mechanism in mechanisms)
        {
            if (mechanism.Value.SpecVersion == this.Pkcs11SpecVersion)
            {
                toRemove.Add(mechanism.Key);
            }
        }

        foreach (CKM mechanism in toRemove)
        {
            mechanisms.Remove(mechanism);
        }
    }
}