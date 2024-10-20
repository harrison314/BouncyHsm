using BouncyHsm.Client;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Pages.SlotsPages;

public class SetPinModel
{
    [Required]
    public CKU UserType
    {
        get;
        set;
    }

    [Required]
    [MinLength(4)]
    public string NewPin
    {
        get;
        set;
    }

    public SetPinModel()
    {
        this.UserType = CKU.CKU_USER;
        this.NewPin = string.Empty;
    }
}
