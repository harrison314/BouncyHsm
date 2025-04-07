using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class UpdateProfileOperation : ProfileOperation
{
    public CKM Mechanism
    {
        get;
        set;
    }

    public uint? MinKeySize
    {
        get;
        set;
    }

    public uint? MaxKeySize
    {
        get;
        set;
    }

    public MechanismCkf[]? Flags
    {
        get;
        set;
    }

    public UpdateProfileOperation()
    {
        
    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        if (!mechanisms.ContainsKey(this.Mechanism))
        {
            throw new BouncyHsmConfigurationException($"Unable to update a mechanism {this.Mechanism} because it not exists in the profile.");
        }

        if (!originalMechanisms.ContainsKey(this.Mechanism))
        {
            throw new BouncyHsmConfigurationException($"The mechanism {this.Mechanism} cannot be added because it is not supported by BouncyHsm.");
        }

        MechanismInfo originalInfo = originalMechanisms[this.Mechanism];
        uint minKeySize = this.MinKeySize ?? originalInfo.MinKeySize;
        uint maxKeySize = this.MaxKeySize ?? originalInfo.MaxKeySize;

        if (minKeySize < originalInfo.MinKeySize)
        {
            throw new BouncyHsmConfigurationException($"The value of {nameof(this.MaxKeySize)} in the mechanism {this.Mechanism} is too low. The value must be in the range supported by BouncyHsm.");
        }

        if (maxKeySize > originalInfo.MaxKeySize)
        {
            throw new BouncyHsmConfigurationException($"The value of {nameof(this.MaxKeySize)} in the mechanism {this.Mechanism} is too high. The value must be in the range supported by BouncyHsm.");

        }

        MechanismCkf concatFlags = MechanismCkf.NONE;
        if (this.Flags != null)
        {
            for (int i = 0; i < this.Flags.Length; i++)
            {
                concatFlags |= this.Flags[i];
            }
        }
        else
        {
            concatFlags = originalInfo.Flags;
        }

        mechanisms[this.Mechanism] = new MechanismInfo(minKeySize,
            maxKeySize,
            concatFlags,
           originalInfo.RequireParamsIn,
           originalInfo.SpecVersion);
    }
}
