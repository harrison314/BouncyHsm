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
        [CommandOption("-d|--slotDecsription <SlotDescription>")]
        public string SlotDescription
        {
            get;
            set;
        } = default!;

        [CommandOption("-l|--tokenLabel <TokenLabel>")]
        public string TokenLabel
        {
            get;
            set;
        } = default!;

        [CommandOption("-s|--tokenSerial <TokenSerial>")]
        public string? TokenSerial
        {
            get;
            set;
        }

        [CommandOption("--simulateHwMechanism")]
        [DefaultValue(true)]
        public bool TokenSimulateHwMechanism
        {
            get;
            init;
        }

        [CommandOption("--simulateHwRng")]
        [DefaultValue(true)]
        public bool TokenSimulateHwRng
        {
            get;
            init;
        }

        [CommandOption("--qualifiedArea")]
        [DefaultValue(false)]
        public bool TokenSimulateQualifiedArea 
        {
            get;
            init;
        }

        //TODO:
        //public SpeedMode SpeedMode
        //{
        //    get;
        //    set;
        //}

        [CommandOption("-u|--userPin <UserPin>")]
        [Required]
        [MinLength(4)]
        public string TokenUserPin
        {
            get;
            set;
        } = default!;

        [CommandOption("-q|--soPin <UserPin>")]
        [Required]
        [MinLength(4)]
        public string TokenSoPin
        {
            get;
            set;
        } = default!;

        [CommandOption("--signaturePin <SignaturePin>")]
        public string? TokenSignaturePin
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
