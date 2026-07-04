using BouncyHsm.Client;
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

    [Required]
    public PkcsDigestAlgorithm SignatureDigestHint
    {
        get;
        set;
    }

    public GenerateCsrModel()
    {
        this.Subject = string.Empty;
        this.SignatureDigestHint = PkcsDigestAlgorithm.SHA256;
    }
}
