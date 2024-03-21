using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Pages.PkcsPages;

public class GenerateSelfSignedCertificateModel
{
    [Required]
    [MinLength(3)]
    public string Subject
    {
        get;
        set;
    }

    [Required]
    public string ValidityInDays
    {
        get;
        set;
    }

    public GenerateSelfSignedCertificateModel()
    {
        this.Subject = string.Empty;
    }
}