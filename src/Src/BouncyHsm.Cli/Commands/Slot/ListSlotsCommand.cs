using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Slot;

internal class ListSlotsCommand : AsyncCommand<ListSlotsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {

    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}
