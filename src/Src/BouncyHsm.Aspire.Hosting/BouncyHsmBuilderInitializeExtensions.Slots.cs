using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithHsmSlotAndToken(this IResourceBuilder<BouncyHsmResource> builder,
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
    public static IResourceBuilder<BouncyHsmResource> WithHsmSlotAndTokenIfNotExists(this IResourceBuilder<BouncyHsmResource> builder,
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
            IList<SlotDto> slots = await client.GetAllSlotsAsync(ct);
            if (!slots.Any(t => t.Token.Label == tokenLabel))
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
            }
        });

        return builder;
    }

    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithHsmSlotAndToken(this IResourceBuilder<BouncyHsmResource> builder,
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
}
