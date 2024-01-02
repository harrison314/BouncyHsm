﻿using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateSecretKeyCommand : AsyncCommand<GenerateSecretKeyCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        [Description("Slot Id.")]
        public int SlotId
        {
            get;
            set;
        }

        [CommandArgument(1, "[CkaLabel]")]
        [Description("CkaLabel.")]
        public string CkaLabel
        {
            get;
            set;
        } = default!;

        [CommandArgument(2, "[CkaId]")]
        [Description("CkaId with prefix 'utf8:' for text representation, 'hex:' for hexadecimal representation and 'base64:' for base64 representation.")]
        public string CkaId
        {
            get;
            set;
        } = default!;

        [CommandArgument(3, "[KeySize]")]
        [Description("Secret key size in bytes.")]
        public int KeySize
        {
            get;
            set;
        }

        [CommandOption("--exportable")]
        [Description("Set CKA_EXPORTABLE to true.")]
        [DefaultValue(false)]
        public bool Exportable
        {
            get;
            set;
        }

        [CommandOption("--forderivation")]
        [Description("Set CKA_DERIVE to true.")]
        [DefaultValue(false)]
        public bool ForDerivation
        {
            get;
            set;
        }

        [CommandOption("--forencryption")]
        [Description("Set CKA_ENCRYPT/CKA_DECRYPT to true.")]
        [DefaultValue(false)]
        public bool ForEncryption
        {
            get;
            set;
        }

        [CommandOption("--forsign")]
        [Description("Set CKA_SIGN/CKA_VERIFY to true.")]
        [DefaultValue(false)]
        public bool ForSigning
        {
            get;
            set;
        }

        [CommandOption("--forwrap")]
        [Description("Set CKA_UNWRAP/CKA_WRAP to true.")]
        [DefaultValue(false)]
        public bool ForWrap
        {
            get;
            set;
        }

        [CommandOption("--senzitive")]
        [Description("Set CKA_SENZITIVE to true.")]
        [DefaultValue(false)]
        public bool Senzitive
        {
            get;
            set;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        await AnsiConsole.Status()
           .StartAsync("Generating...", async ctx =>
           {
               await client.GenerateSecretKeyAsync(settings.SlotId, new GenerateSecretKeyRequestDto()
               {
                   Size = settings.KeySize,
                   KeyAttributes = new GenerateKeyAttributesDto()
                   {
                       CkaId = CkaIdParser.Parse(settings.CkaId),
                       CkaLabel = settings.CkaLabel,

                       Exportable = settings.Exportable,
                       ForDerivation = settings.ForDerivation,
                       ForEncryption = settings.ForEncryption,
                       ForSigning = settings.ForSigning,
                       ForWrap = settings.ForWrap,
                       Senzitive = settings.Senzitive,
                   }
               });
           });

        AnsiConsole.MarkupLine("Secret key has created.");
        return 0;
    }
}