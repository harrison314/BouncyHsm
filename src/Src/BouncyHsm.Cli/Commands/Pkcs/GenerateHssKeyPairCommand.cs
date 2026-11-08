using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateHssKeyPairCommand : AsyncCommand<GenerateHssKeyPairCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        [Description("Slot Id.")]
        public required int SlotId
        {
            get;
            init;
        }

        [CommandArgument(1, "[CkaLabel]")]
        [Description("CkaLabel.")]
        public required string CkaLabel
        {
            get;
            init;
        }

        [CommandArgument(2, "[CkaId]")]
        [Description("CkaId with prefix 'utf8:' for text representation, 'hex:' for hexadecimal representation and 'base64:' for base64 representation.")]
        public required string CkaId
        {
            get;
            init;
        }

        [CommandOption("--levels")]
        [Description("Number of HSS levels (1-8).")]
        [DefaultValue(1)]
        public int Levels
        {
            get;
            init;
        }

        [CommandOption("--lms-types")]
        [Description("Comma separated list of LMS types, one per level (LMS_SHA256_M32_H5, LMS_SHA256_M32_H10, LMS_SHA256_M32_H15, LMS_SHA256_M32_H20, LMS_SHA256_M32_H25, LMS_SHA256_M24_H5, LMS_SHA256_M24_H10, LMS_SHA256_M24_H15, LMS_SHA256_M24_H20, LMS_SHA256_M24_H25, LMS_SHAKE_M32_H5, LMS_SHAKE_M32_H10, LMS_SHAKE_M32_H15, LMS_SHAKE_M32_H20, LMS_SHAKE_M32_H25, LMS_SHAKE_M24_H5, LMS_SHAKE_M24_H10, LMS_SHAKE_M24_H15, LMS_SHAKE_M24_H20, LMS_SHAKE_M24_H25). Large heights can take minutes to generate.")]
        public required string LmsTypes
        {
            get;
            init;
        }

        [CommandOption("--lmots-types")]
        [Description("Comma separated list of LM-OTS types, one per level (LMOTS_SHA256_N32_W1, LMOTS_SHA256_N32_W2, LMOTS_SHA256_N32_W4, LMOTS_SHA256_N32_W8, LMOTS_SHA256_N24_W1, LMOTS_SHA256_N24_W2, LMOTS_SHA256_N24_W4, LMOTS_SHA256_N24_W8, LMOTS_SHAKE_N32_W1, LMOTS_SHAKE_N32_W2, LMOTS_SHAKE_N32_W4, LMOTS_SHAKE_N32_W8, LMOTS_SHAKE_N24_W1, LMOTS_SHAKE_N24_W2, LMOTS_SHAKE_N24_W4, LMOTS_SHAKE_N24_W8).")]
        public required string LmotsTypes
        {
            get;
            init;
        }

        [CommandOption("--forsign")]
        [Description("Set CKA_SIGN/CKA_VERIFY to true.")]
        [DefaultValue(false)]
        public bool ForSigning
        {
            get;
            init;
        }

        public override ValidationResult Validate()
        {
            if (this.Levels < 1)
            {
                return ValidationResult.Error("--levels must be greater than zero.");
            }

            return base.Validate();
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        string[] lmsTypes = this.ParseList(settings.LmsTypes);
        string[] lmotsTypes = this.ParseList(settings.LmotsTypes);

        GeneratedKeyPairIdsDto? ids = null;
        await AnsiConsole.Status()
           .StartAsync("Generating...", async ctx =>
           {
               ids = await client.GenerateHssKeyPairAsync(settings.SlotId, new GenerateHssKeyPairRequestDto()
               {
                   Levels = settings.Levels,
                   LmsTypes = lmsTypes,
                   LmotsTypes = lmotsTypes,
                   KeyAttributes = new GenerateKeyAttributesDto()
                   {
                       CkaId = CkaIdParser.Parse(settings.CkaId),
                       CkaLabel = settings.CkaLabel,

                       Exportable = false,
                       ForDerivation = false,
                       ForEncryption = false,
                       ForEncapsulation = false,
                       ForSigning = settings.ForSigning,
                       ForWrap = false,
                       Sensitive = true,
                   }
               }, cancellationToken);
           });

        AnsiConsole.MarkupLine("Key pair has created.");
        if (ids != null)
        {
            AnsiConsole.MarkupLine($"Public key id: {ids.PublicKeyId}");
            AnsiConsole.MarkupLine($"Private key id: {ids.PrivateKeyId}");
        }

        return 0;
    }

    private string[] ParseList(string text)
    {
        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
