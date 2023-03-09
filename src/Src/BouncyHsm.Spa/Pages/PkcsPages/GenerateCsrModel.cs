using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Pages.PkcsPages;

public class GenerateCsrModel
{
    [Required]
    [MinLength(3)]
    public string Subject
    {
        get;
        set;
    }

    public GenerateCsrModel()
    {
        this.Subject = string.Empty;
    }
}
