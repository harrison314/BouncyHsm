using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                new System.Text.Json.JsonSerializerOptions()
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                    UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow
                });
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

            return new MechnismProfile(newDictionary, profile.Name);

        },
        profile);
    }
}
