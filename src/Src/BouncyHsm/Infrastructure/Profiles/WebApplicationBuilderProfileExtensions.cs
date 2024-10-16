using BouncyHsm.Core.Services.Profiles;
using BouncyHsm.Services.Configuration;

namespace BouncyHsm;

internal static class WebApplicationBuilderProfileExtensions
{
    public static void TryUseProfileFromConfiguration(this WebApplicationBuilder builder)
    {
        string? path = builder.Configuration[$"{nameof(BouncyHsmSetup)}:{nameof(BouncyHsmSetup.ProfileFilePath)}"];

        if (!string.IsNullOrEmpty(path))
        {
            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ProfileUpdater.UpdateProfile(fs);
        }
    }
}
