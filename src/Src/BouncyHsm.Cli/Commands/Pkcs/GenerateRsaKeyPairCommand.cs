using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateRsaKeyPairCommand : AsyncCommand<GenerateRsaKeyPairCommand.Settings>
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

        [CommandArgument(3, "[KeySize]")]
        [Description("RSA key size (eg. 2048).")]
        public required int KeySize
        {
            get;
            init;
        }

        [CommandOption("--exportable")]
        [Description("Set CKA_EXPORTABLE to true.")]
        [DefaultValue(false)]
        public bool Exportable
        {
            get;
            init;
        }

        [CommandOption("--forderivation")]
        [Description("Set CKA_DERIVE to true.")]
        [DefaultValue(false)]
        public bool ForDerivation
        {
            get;
            init;
        }

        [CommandOption("--forencryption")]
        [Description("Set CKA_ENCRYPT/CKA_DECRYPT to true.")]
        [DefaultValue(false)]
        public bool ForEncryption
        {
            get;
            init;
        }

        [CommandOption("--forencapsulation")]
        [Description("Set CKA_ENCAPSULATE/CKA_DECAPSULATE to true.")]
        [DefaultValue(false)]
        public bool ForEncapsulation
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

        [CommandOption("--forwrap")]
        [Description("Set CKA_UNWRAP/CKA_WRAP to true.")]
        [DefaultValue(false)]
        public bool ForWrap
        {
            get;
            init;
        }

        [CommandOption("--sensitive")]
        [Description("Set CKA_SENSITIVE to true.")]
        [DefaultValue(false)]
        public bool Sensitive
        {
            get;
            init;
        }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        await AnsiConsole.Status()
           .StartAsync("Generating...", async ctx =>
           {
               await client.GenerateRsaKeyPairAsync(settings.SlotId, new GenerateRsaKeyPairRequestDto()
               {
                   KeySize = settings.KeySize,
                   KeyAttributes = new GenerateKeyAttributesDto()
                   {
                       CkaId = CkaIdParser.Parse(settings.CkaId),
                       CkaLabel = settings.CkaLabel,

                       Exportable = settings.Exportable,
                       ForDerivation = settings.ForDerivation,
                       ForEncryption = settings.ForEncryption,
                       ForEncapsulation = settings.ForEncapsulation,
                       ForSigning = settings.ForSigning,
                       ForWrap = settings.ForWrap,
                       Sensitive = settings.Sensitive,
                   }
               }, cancellationToken);
           });

        AnsiConsole.MarkupLine("Key pair has created.");
        return 0;
    }
}
