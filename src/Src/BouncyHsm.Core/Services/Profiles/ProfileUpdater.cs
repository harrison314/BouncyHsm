using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Profiles;

public static class ProfileUpdater
{
    public static void UpdateProfile(Stream stream)
    {
        System.Diagnostics.Debug.Assert(stream != null);

        Profile? profile;

        try
        {
            profile = System.Text.Json.JsonSerializer.Deserialize<Profile>(stream,
                 ProfileJsonContext.Default.Profile);
        }
        catch (Exception ex)
        {
            throw new BouncyHsmConfigurationException($"Problem with reading profile: {ex.Message}", ex);
        }

        if (profile == null)
        {
            throw new BouncyHsmConfigurationException("Can not read profile.");
        }

        if (string.IsNullOrEmpty(profile.Name))
        {
            throw new BouncyHsmConfigurationException("The profile must have a completed name.");
        }

        MechanismUtils.UpdateMechanisms((originalMechanisms, profile) =>
        {
            Dictionary<CKM, MechanismInfo> newDictionary = new Dictionary<CKM, MechanismInfo>(originalMechanisms);
            foreach (ProfileOperation operation in profile.Operations)
            {
                operation.Update(ref newDictionary, originalMechanisms);
            }

            return new MechanismProfile(newDictionary, profile.Name);

        },
        profile);

        if (profile.EnabledCurves != null)
        {
            EcdsaUtils.SetEnabledCurves(profile.EnabledCurves);
        }
    }
}
