using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class RemoveProfileOperation : ProfileOperation
{
    public CKM Mechanism
    {
        get;
        set;
    }

    public RemoveProfileOperation()
    {
        
    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        if (!mechanisms.Remove(this.Mechanism))
        {
            throw new BouncyHsmConfigurationException($"Can nor remove mechanism {this.Mechanism} because it was not found in profile."); //TODO
        }
    }
}

