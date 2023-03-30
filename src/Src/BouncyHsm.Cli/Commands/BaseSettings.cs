using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands;

internal class BaseSettings : CommandSettings
{
    [Description("BouncyHsm REST API endpoint.")]
    [CommandOption("-e|--endpoint")]
    public string Endpoint
    {
        get;
        init;
    } = default!;
}
