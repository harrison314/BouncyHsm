using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class AddProfileOperation : ProfileOperation
{
    public CKM Mechanism
    {
        get;
        set;
    }

    public uint MinKeySize
    {
        get;
        set;
    }

    public uint MaxKeySize
    {
        get;
        set;
    }

    public MechanismCkf[] Flags
    {
        get;
        set;
    }

    public AddProfileOperation()
    {
        this.Flags = Array.Empty<MechanismCkf>();
    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        if (mechanisms.ContainsKey(this.Mechanism))
        {
            throw new BouncyHsmConfigurationException($"Unable to add a mechanism {this.Mechanism} because it already exists in the profile.");
        }

        if (!originalMechanisms.ContainsKey(this.Mechanism))
        {
            throw new BouncyHsmConfigurationException($"The mechanism {this.Mechanism} cannot be added because it is not supported by BouncyHsm.");
        }

        MechanismInfo originalInfo = originalMechanisms[this.Mechanism];
        if (this.MinKeySize < originalInfo.MinKeySize)
        {
            throw new BouncyHsmConfigurationException($"The value of {nameof(this.MaxKeySize)} in the mechanism {this.Mechanism} is too low. The value must be in the range supported by BouncyHsm.");
        }

        if (this.MaxKeySize > originalInfo.MaxKeySize)
        {
            throw new BouncyHsmConfigurationException($"The value of {nameof(this.MaxKeySize)} in the mechanism {this.Mechanism} is too high. The value must be in the range supported by BouncyHsm.");

        }

        MechanismCkf concatFlags = MechanismCkf.NONE;
        for (int i = 0; i < this.Flags.Length; i++)
        {
            concatFlags |= this.Flags[i];
        }

        mechanisms.Add(this.Mechanism, new MechanismInfo(this.MinKeySize,
            this.MaxKeySize,
            concatFlags,
            originalInfo.RequireParamsIn,
            originalInfo.HasOptionalParams,
            originalInfo.SpecVersion));
    }
}

