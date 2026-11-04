using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithImportPkcs12(this IResourceBuilder<BouncyHsmResource> builder,
        string tokenLabel,
        byte[] pkcs12Content,
        string password,
        byte[] ckaId,
        string ckaLabel,
        bool importCertificateChain = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrEmpty(tokenLabel);
        ArgumentNullException.ThrowIfNull(pkcs12Content);
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(ckaId);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(ckaLabel);

        builder.Resource.InitializeTasks.Add(async (client, logger, ct) =>
        {
            IList<SlotDto> slots = await client.GetAllSlotsAsync(ct);
            SlotDto? slot = slots.SingleOrDefault(t => t.Token.Label == tokenLabel);
            if (slot == null)
            {
                throw new InvalidOperationException($"Token with label '{tokenLabel}' not found. Make sure the token is created before importing PKCS#12 objects.");
            }

            await client.ImportP12Async(slot.SlotId,
                new ImportP12RequestDto()
                {
                    CkaId = ckaId,
                    CkaLabel = ckaLabel,
                    ImportChain = importCertificateChain,
                    ImportMode = PrivateKeyImportMode.Local,
                    Password = password,
                    Pkcs12Content = pkcs12Content
                },
                ct);

            logger.LogInformation("Pkcs12 imported into token with label {TokenLabel}, CKA_ID {CkaID}, CKA_LABEL {CkaLabel}.",
                    tokenLabel,
                    Convert.ToHexString(ckaId),
                    ckaLabel);
        });

        return builder;
    }

    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithImportPkcs12IfNotExists(this IResourceBuilder<BouncyHsmResource> builder,
        string tokenLabel,
        byte[] pkcs12Content,
        string password,
        byte[] ckaId,
        string ckaLabel,
        bool importCertificateChain = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNullOrEmpty(tokenLabel);
        ArgumentNullException.ThrowIfNull(pkcs12Content);
        ArgumentNullException.ThrowIfNullOrEmpty(password);
        ArgumentNullException.ThrowIfNull(ckaId);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(ckaLabel);

        builder.Resource.InitializeTasks.Add(async (client, logger, ct) =>
        {
            IList<SlotDto> slots = await client.GetAllSlotsAsync(ct);
            SlotDto? slot = slots.SingleOrDefault(t => t.Token.Label == tokenLabel);
            if (slot == null)
            {
                throw new InvalidOperationException($"Token with label '{tokenLabel}' not found. Make sure the token is created before importing PKCS#12 objects.");
            }

            PkcsObjectsDto objects = await client.GetPkcsObjectsAsync(slot.SlotId, ct);
            if (!objects.Objects.Any(t => t.CkaLabel == ckaLabel && t.CkaId.SequenceEqual(ckaId)))
            {
                await client.ImportP12Async(slot.SlotId,
                    new ImportP12RequestDto()
                    {
                        CkaId = ckaId,
                        CkaLabel = ckaLabel,
                        ImportChain = importCertificateChain,
                        ImportMode = PrivateKeyImportMode.Local,
                        Password = password,
                        Pkcs12Content = pkcs12Content
                    },
                    ct);

                logger.LogInformation("Pkcs12 imported into token with label {TokenLabel}, CKA_ID {CkaID}, CKA_LABEL {CkaLabel}.",
                    tokenLabel,
                    Convert.ToHexString(ckaId),
                    ckaLabel);
            }
            else
            {
                logger.LogWarning("The import of the PKCS#12 file into the token labeled {TokenLabel} failed because another cryptographic object with CKA_ID {CkaID} and CKA_LABEL {CkaLabel} already exists.",
                    tokenLabel,
                    Convert.ToHexString(ckaId),
                    ckaLabel);
            }
        });

        return builder;
    }
}