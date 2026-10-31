using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;

namespace Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    public static IResourceBuilder<BouncyHsmResource> WithImportPkcs12(this IResourceBuilder<BouncyHsmResource> builder,
        string tokenLabel,
        byte[] pkcs12Content,
        string password,
        byte[] ckaId,
        string ckaLabel,
        bool impotCertificateChain = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrEmpty(tokenLabel);
        ArgumentNullException.ThrowIfNull(pkcs12Content);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(ckaId);
        ArgumentNullException.ThrowIfNull(ckaLabel);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            IList<SlotDto> slots = await client.GetAllSlotsAsync(ct);
            SlotDto? slot = slots.SingleOrDefault(t => t.Token.Label == tokenLabel);
            if (slot == null)
            {
                throw new ArgumentException($"Not found token with label {tokenLabel}");
            }

            await client.ImportP12Async(slot.SlotId,
                new ImportP12RequestDto()
                {
                    CkaId = ckaId,
                    CkaLabel = ckaLabel,
                    ImportChain = impotCertificateChain,
                    ImportMode = PrivateKeyImportMode.Local,
                    Password = password,
                    Pkcs12Content = pkcs12Content
                },
                ct);
        });

        return builder;
    }

    public static IResourceBuilder<BouncyHsmResource> WithImportPkcs12IfNotExists(this IResourceBuilder<BouncyHsmResource> builder,
        string tokenLabel,
        byte[] pkcs12Content,
        string password,
        byte[] ckaId,
        string ckaLabel,
        bool impotCertificateChain = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrEmpty(tokenLabel);
        ArgumentNullException.ThrowIfNull(pkcs12Content);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(ckaId);
        ArgumentNullException.ThrowIfNull(ckaLabel);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            IList<SlotDto> slots = await client.GetAllSlotsAsync(ct);
            SlotDto? slot = slots.SingleOrDefault(t => t.Token.Label == tokenLabel);
            if (slot == null)
            {
                throw new ArgumentException($"Not found token with label {tokenLabel}");
            }

            PkcsObjectsDto objects = await client.GetPkcsObjectsAsync(slot.SlotId, ct);
            if (!objects.Objects.Any(t => t.CkaLabel == ckaLabel && t.CkaId.SequenceEqual(ckaId)))
            {
                await client.ImportP12Async(slot.SlotId,
                    new ImportP12RequestDto()
                    {
                        CkaId = ckaId,
                        CkaLabel = ckaLabel,
                        ImportChain = impotCertificateChain,
                        ImportMode = PrivateKeyImportMode.Local,
                        Password = password,
                        Pkcs12Content = pkcs12Content
                    },
                    ct);
            }
        });

        return builder;
    }
}