using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class EnableProfileOperation : ProfileOperation
{
    public CKM Mechanism
    {
        get;
        set;
    }

    public EnableProfileOperation()
    {

    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        if (!originalMechanisms.ContainsKey(this.Mechanism))
        {
            throw new BouncyHsmConfigurationException($"The mechanism {this.Mechanism} cannot be added because it is not supported by BouncyHsm.");
        }

        mechanisms[this.Mechanism] = originalMechanisms[this.Mechanism];
    }
}
