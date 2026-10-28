using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public static class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithDbMigration<T>(this IResourceBuilder<BouncyHsmResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            await client.MigrateAsync(new BouncyHsm.Client.MigrationRequestDto()
            {
                ResetAllowedMechanism = false
            }, ct);
        });

        return builder;
    }

    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithHsmSlot<T>(this IResourceBuilder<BouncyHsmResource> builder,
        string slotDescription,
        string tokenLabel,
        string userPin,
        string soPin,
        string? tokenSerialNumber = null,
        string? signaturePin = null,
        bool isRemovableDevice = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(slotDescription);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(tokenLabel);
        ArgumentNullException.ThrowIfNullOrEmpty(userPin);
        ArgumentNullException.ThrowIfNullOrEmpty(soPin);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            await client.CreateSlotAsync(new BouncyHsm.Client.CreateSlotDto()
            {
                Description = slotDescription,
                IsHwDevice = true,
                IsRemovableDevice = isRemovableDevice,
                Token = new BouncyHsm.Client.CreateTokenDto()
                {
                    Label = tokenLabel,
                    SerialNumber = tokenSerialNumber,
                    SignaturePin = signaturePin,
                    SimulateHwMechanism = true,
                    SimulateHwRng = true,
                    SimulateProtectedAuthPath = false,
                    SimulateQualifiedArea = !string.IsNullOrEmpty(signaturePin),
                    SoPin = soPin,
                    SpeedMode = BouncyHsm.Client.SpeedMode.WithoutRestriction,
                    UserPin = userPin
                }
            }, ct);
        });

        return builder;
    }

    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithHsmSlot<T>(this IResourceBuilder<BouncyHsmResource> builder,
        Action<BouncyHsm.Client.CreateSlotDto> setup)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setup);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            BouncyHsm.Client.CreateSlotDto model = new BouncyHsm.Client.CreateSlotDto()
            {
                Description = null!,
                IsHwDevice = true,
                IsRemovableDevice = false,
                Token = new BouncyHsm.Client.CreateTokenDto()
                {
                    Label = null!,
                    SerialNumber = null!,
                    SignaturePin = null!,
                    SimulateHwMechanism = true,
                    SimulateHwRng = true,
                    SimulateProtectedAuthPath = false,
                    SimulateQualifiedArea = false,
                    SoPin = null!,
                    SpeedMode = BouncyHsm.Client.SpeedMode.WithoutRestriction,
                    UserPin = null!
                }
            };

            setup.Invoke(model);
            await client.CreateSlotAsync(model, ct);
        });

        return builder;
    }

    //TODO: slot initialize,
    // PEM import, P12 import
    // migrate DB
}