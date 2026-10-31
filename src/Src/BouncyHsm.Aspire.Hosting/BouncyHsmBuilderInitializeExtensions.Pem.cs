using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;

namespace Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithImportPemObjects(this IResourceBuilder<BouncyHsmResource> builder,
        string tokenLabel,
        byte[] ckaId,
        string ckaLabel,
        string pem,
        BouncyHsmPemHint hints = BouncyHsmPemHint.None,
        string? password = null
        )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrEmpty(tokenLabel);
        ArgumentNullException.ThrowIfNull(ckaId);
        ArgumentNullException.ThrowIfNull(ckaLabel);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(pem);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            IList<SlotDto> slots = await client.GetAllSlotsAsync(ct);
            SlotDto? slot = slots.SingleOrDefault(t => t.Token.Label == tokenLabel);
            if (slot == null)
            {
                throw new ArgumentException($"Not found token with label {tokenLabel}");
            }

            await client.ImportPemAsync(slot.SlotId,
                new ImportPemRequestDto()
                {
                    CkaId = ckaId,
                    CkaLabel = ckaLabel,
                    Pem = pem,
                    Hints = new ImportPemHintsDto()
                    {
                        ForDerivation = hints.HasFlag(BouncyHsmPemHint.ForDerivation),
                        ForEncapsulation = hints.HasFlag(BouncyHsmPemHint.ForEncapsulation),
                        ForEncryption = hints.HasFlag(BouncyHsmPemHint.ForEncryption),
                        ForSigning = hints.HasFlag(BouncyHsmPemHint.ForSigning),
                        ForWrap = hints.HasFlag(BouncyHsmPemHint.ForWrap),
                        ImportMode = PrivateKeyImportMode.Local
                    },
                    Password = password
                },
                ct);
        });

        return builder;
    }
}
