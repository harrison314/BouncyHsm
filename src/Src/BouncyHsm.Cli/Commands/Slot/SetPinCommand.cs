using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Slot;

internal class SetPinCommand : AsyncCommand<SetPinCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        public required int SlotId
        {
            get;
            init;
        }

        [CommandArgument(1, "[CKU]")]
        [Description($"User type. Allowed values are {nameof(CKU.CKU_USER)}, {nameof(CKU.CKU_SO)}, {nameof(CKU.CKU_CONTEXT_SPECIFIC)}.")]
        public required CKU UserType
        {
            get;
            init;
        }

        [CommandOption("-u|--pin <PIN>")]
        [DefaultValue(null)]
        [Description("PIN for token. (Optional parameter)")]
        public string? NewPin
        {
            get;
            init;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        string newPin;

        if (string.IsNullOrEmpty(settings.NewPin))
        {
            newPin = AnsiConsole.Prompt(new TextPrompt<string>($"Enter [green]{settings.UserType} PIN[/]:").Secret());
        }
        else
        {
            newPin = settings.NewPin;
        }

        await AnsiConsole.Status()
               .StartAsync("Set token pin...", async ctx =>
               {
                   await client.SetTokenPinForSlotAsync(settings.SlotId,
                       new SetTokenPinDataDto()
                       {
                           UserType = settings.UserType,
                           NewPin = newPin
                       }, cancellationToken);
               });

        AnsiConsole.MarkupLine("Set new PIN for token in slot with id [green]{0}[/] for user type [green]{1}[/].",
            settings.SlotId,
            settings.UserType);
        return 0;
    }
}