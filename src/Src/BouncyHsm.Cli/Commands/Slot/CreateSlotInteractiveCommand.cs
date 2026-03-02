using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace BouncyHsm.Cli.Commands.Slot;

internal class CreateSlotInteractiveCommand : AsyncCommand<CreateSlotInteractiveCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {

    }

    internal sealed class Features
    {
        public const string TokenSimulateHwMechanism = "HW Mechanisms";
        public const string IsRemovableDevice = "Removable Device";
        public const string SimulateHwRng = "HW RNG";
        public const string SimulateQualifiedArea = "Qualified Area";
        public const string SimulateProtectedAuthPath = "Protected Auth Path";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("a new slot with token in interactive mode");

        this.WriteRule("Token informations");

        string tokenLabel = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]token label[/]:"));
        string description = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]token description[/]:"));

        this.WriteRule("Features");

        List<string> selected = AnsiConsole.Prompt(
          new MultiSelectionPrompt<string>()
         .Title("Enable token [green]features[/]")
         .AddChoices(Features.TokenSimulateHwMechanism,
             Features.IsRemovableDevice,
             Features.SimulateHwRng,
             Features.SimulateQualifiedArea,
             Features.SimulateProtectedAuthPath)
         .Select(Features.TokenSimulateHwMechanism)
         .Select(Features.SimulateHwRng));

        string speedModeStr = AnsiConsole.Prompt(new SelectionPrompt<string>()
           .Title("Select [green]speed mode[/]:")
           .AddChoices(nameof(SpeedMode.WithoutRestriction), nameof(SpeedMode.Hsm), nameof(SpeedMode.SmartCard)));

        AnsiConsole.MarkupLine($"Enabled features:");
        foreach (string feature in selected)
        {
            AnsiConsole.MarkupLine($" - [green]{feature}[/]");
        }

        AnsiConsole.MarkupLine($"Speed mode: [green]{speedModeStr}[/]");

        this.WriteRule("Token PINs");

        string userPin = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]user PIN[/]:").Secret());
        string soPin = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]so PIN[/]:").Secret());
        string? signaturePin = null;

        if (selected.Contains(Features.SimulateQualifiedArea))
        {
            signaturePin = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]signature PIN[/]:").Secret());
        }

        this.WriteRule("Finish");

        if (AnsiConsole.Confirm("Do you really want to create a new slot with the specified parameters?"))
        {
            AnsiConsole.WriteLine();

            CreateSlotResultDto result = default!;

            IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
            await AnsiConsole.Status()
              .StartAsync("Creating...", async ctx =>
              {
                  result = await client.CreateSlotAsync(new CreateSlotDto()
                  {
                      IsHwDevice = selected.Contains(Features.TokenSimulateHwMechanism),
                      IsRemovableDevice = selected.Contains(Features.IsRemovableDevice),
                      Description = description.Trim(),
                      Token = new CreateTokenDto()
                      {
                          Label = tokenLabel.Trim(),
                          SerialNumber = null,
                          SimulateHwMechanism = selected.Contains(Features.TokenSimulateHwMechanism),
                          SimulateHwRng = selected.Contains(Features.SimulateHwRng),
                          SimulateQualifiedArea = selected.Contains(Features.SimulateQualifiedArea),
                          SpeedMode = Enum.Parse<SpeedMode>(speedModeStr),
                          UserPin = userPin,
                          SoPin = soPin,
                          SignaturePin = signaturePin,
                          SimulateProtectedAuthPath = selected.Contains(Features.SimulateProtectedAuthPath)
                      }
                  }, cancellationToken);
              });

            AnsiConsole.MarkupLine("Created new slot with id [green]{0}[/] and token serial [green]{1}[/].",
                result.Id,
                result.TokenSerialNumber);
        }

        return 0;
    }

    private void WriteRule(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[blue]{title}[/]").LeftJustified());
    }
}
