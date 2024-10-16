using BouncyHsm.Core.UseCases.Contracts;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.ImportPemHints))]
public class ImportPemHintsDto
{
    [Required]
    public bool ForSigning
    {
        get;
        set;
    }

    [Required]
    public bool ForEncryption
    {
        get;
        set;
    }

    [Required]
    public bool ForDerivation
    {
        get;
        set;
    }

    [Required]
    public bool ForWrap
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

    public ImportPemHintsDto()
    {
        
    }
}
