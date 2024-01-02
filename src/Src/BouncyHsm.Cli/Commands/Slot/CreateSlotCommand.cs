using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Slot;

internal class CreateSlotCommand : AsyncCommand<CreateSlotCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandOption("-d|--slotDescription <SlotDescription>")]
        [Description("Slot description.")]
        public string SlotDescription
        {
            get;
            set;
        } = default!;

        [CommandOption("-l|--tokenLabel <TokenLabel>")]
        [Description("Token label.")]
        public string TokenLabel
        {
            get;
            set;
        } = default!;

        [CommandOption("-s|--tokenSerial <TokenSerial>")]
        [Description("Token serial number. If not set it is created automatically. (Optional parameter)")]
        public string? TokenSerial
        {
            get;
            set;
        }

        [CommandOption("--simulateHwMechanism")]
        [DefaultValue(true)]
        [Description("If set to true, then the slot and token is presented as a HW device.")]
        public bool TokenSimulateHwMechanism
        {
            get;
            init;
        }

        [CommandOption("--simulateHwRng")]
        [DefaultValue(true)]
        [Description("If set to true, then the token simulate HW RNG.")]
        public bool TokenSimulateHwRng
        {
            get;
            init;
        }

        [CommandOption("--qualifiedArea")]
        [DefaultValue(false)]
        [Description("If set to true, the token allows you to use a signature PIN.")]
        public bool TokenSimulateQualifiedArea
        {
            get;
            init;
        }

        [CommandOption("--speedMode <SpeedMode>")]
        [DefaultValue(SpeedMode.WithoutRestriction)]
        [Description("Speed mode. Allowed values are WithoutRestriction, Hsm and SmartCard.")]
        public SpeedMode SpeedMode
        {
            get;
            set;
        }

        [CommandOption("-u|--userPin <UserPin>")]
        [DefaultValue(null)]
        [Description("User PIN for token. (Optional parameter)")]
        public string? TokenUserPin
        {
            get;
            set;
        }

        [CommandOption("-q|--soPin <UserPin>")]
        [DefaultValue(null)]
        [Description("SO PIN for token. (Optional parameter)")]
        public string? TokenSoPin
        {
            get;
            set;
        }

        [CommandOption("--signaturePin <SignaturePin>")]
        [DefaultValue(null)]
        [Description("Signature PIN for token. (Optional parameter)")]
        public string? TokenSignaturePin
        {
            get;
            set;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        string userPin;
        string soPin;
        string? signaturePin = null;

        if (string.IsNullOrEmpty(settings.TokenUserPin))
        {
            userPin = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]user PIN[/]:").Secret());
        }
        else
        {
            userPin = settings.TokenUserPin;
        }

        if (string.IsNullOrEmpty(settings.TokenSoPin))
        {
            soPin = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]so PIN[/]:").Secret());
        }
        else
        {
            soPin = settings.TokenSoPin;
        }

        if (settings.TokenSimulateQualifiedArea)
        {
            if (string.IsNullOrEmpty(settings.TokenSignaturePin))
            {
                signaturePin = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]so PIN[/]:").Secret());
            }
            else
            {
                signaturePin = settings.TokenSignaturePin;
            }
        }

        CreateSlotResultDto result = default!;

        await AnsiConsole.Status()
          .StartAsync("Creating...", async ctx =>
          {
              result = await client.CreateSlotAsync(new CreateSlotDto()
              {
                  IsHwDevice = settings.TokenSimulateHwMechanism,
                  Description = settings.SlotDescription.Trim(),
                  Token = new CreateTokenDto()
                  {
                      Label = settings.TokenLabel.Trim(),
                      SerialNumber = settings.TokenSerial?.Trim(),
                      SimulateHwMechanism = settings.TokenSimulateHwMechanism,
                      SimulateHwRng = settings.TokenSimulateHwRng,
                      SimulateQualifiedArea = settings.TokenSimulateQualifiedArea,
                      SpeedMode = settings.SpeedMode,
                      UserPin = userPin,
                      SoPin = soPin,
                      SignaturePin = signaturePin
                  }
              });
          });

        AnsiConsole.MarkupLine("Created new slot with id [green]{0}[/] and token serial [green]{1}[/].",
            result.Id,
            result.TokenSerialNumber);

        return 0;
    }
}
