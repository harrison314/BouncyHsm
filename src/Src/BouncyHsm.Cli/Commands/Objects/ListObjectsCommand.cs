﻿using BouncyHsm.Cli.Commands.Slot;
using BouncyHsm.Spa.Services.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Objects;

internal class ListObjectsCommand : AsyncCommand<ListObjectsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        public int SlotId
        {
            get;
            set;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        BouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        StorageObjectsListDto objects = default!;

        await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                objects = await client.GetStorageObjectsAsync(settings.SlotId, null, null);
            });

        Table table = new Table();
        table.AddColumn("Id");
        table.AddColumn("CkaLabel");
        table.AddColumn("CkaId");
        table.AddColumn("CKO");
        table.AddColumn("CKK");
        table.AddColumn("Description");

        foreach (StorageObjectInfoDto info in objects.Objects)
        {
            table.AddRow(new Markup($"[green]{info.Id}[/]"),
                new Markup(Markup.Escape(info.CkLabel)),
                new Markup(Markup.Escape(info.CkIdHex ?? string.Empty)),
                new Markup(Markup.Escape(info.Type.ToString())),
                new Markup(Markup.Escape(info.KeyType?.ToString() ?? "-")),
                new Markup(Markup.Escape(info.Description)));
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
