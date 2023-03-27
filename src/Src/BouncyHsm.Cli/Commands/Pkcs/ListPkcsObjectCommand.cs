using BouncyHsm.Cli.Commands.Objects;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ListPkcsObjectCommand : AsyncCommand<ListPkcsObjectCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "SlotId")]
        public int SlotId
        {
            get;
            set;
        }
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}
