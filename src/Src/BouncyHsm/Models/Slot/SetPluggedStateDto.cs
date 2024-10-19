using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.Slot;

public class SetPluggedStateDto
{
    [Required]
    public bool Plugged
    {
        get;
        set;
    }

    public SetPluggedStateDto()
    {
        
    }
}
