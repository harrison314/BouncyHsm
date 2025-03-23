using BouncyHsm.Core.UseCases.Contracts;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.ImportP12Request), IgnoredMembers = new string[] { "SlotId" })]
public class ImportP12RequestDto
{
    [Required]
    [MaxLength(1024)]
    public string CkaLabel
    {
        get;
        set;
    }

    [Required]
    [MaxLength(1024)]
    public byte[] CkaId
    {
        get;
        set;
    }

    [Required]
    public PrivateKeyImportMode ImportMode
    {
        get;
        set;
    }

    public bool ImportChain
    {
        get;
        set;
    }

    [Required]
    [MaxLength(1024 * 1024)]
    public byte[] Pkcs12Content
    {
        get;
        set;
    }

    [Required(AllowEmptyStrings = true)]
    [MaxLength(1024)]
    public string Password
    {
        get;
        set;
    }

    public ImportP12RequestDto()
    {
        this.CkaLabel = string.Empty;
        this.CkaId = Array.Empty<byte>();
        this.Pkcs12Content = Array.Empty<byte>();
        this.Password = string.Empty;
    }
}
