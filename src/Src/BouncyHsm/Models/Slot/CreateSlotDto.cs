using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Models.Slot;

public class CreateSlotDto
{
    public bool IsHwDevice
    {
        get;
        set;
    }

    [Required]
    [MinLength(1)]
    [MaxLength(512)]
    public string Description
    {
        get;
        set;
    }

    public CreateTokenDto Token
    {
        get;
        set;
    }

    public CreateSlotDto()
    {
        this.Description = string.Empty;
        this.Token = default!;
    }
}
