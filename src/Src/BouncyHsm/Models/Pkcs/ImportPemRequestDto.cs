using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.ImportPemRequest), IgnoredMembers = new string[] { "SlotId" })]
public class ImportPemRequestDto
{
    [Required]
    public uint SlotId
    {
        get;
        set;
    }

    [Required]
    [MaxLength(102400)]
    public string Pem
    {
        get;
        set;
    }

    [MaxLength(1024)]
    public string? Password
    {
        get;
        set;
    }

    [Required]
    [MaxLength(1024)]
    public string CkaLabel
    {
        get;
        set;
    }

    [MaxLength(1024)]
    public byte[]? CkaId
    {
        get;
        set;
    }

    [Required]
    public ImportPemHintsDto Hints
    {
        get;
        set;
    }

    public ImportPemRequestDto()
    {
        this.CkaLabel = string.Empty;
        this.Pem = string.Empty;
        this.Hints = new ImportPemHintsDto();
    }
}
