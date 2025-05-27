using BouncyHsm.Client;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.StorageObjects;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.HighLevelAttributeValue))]
public class HighLevelAttributeValueDto
{
    [Required]
    public AttrTypeTag TypeTag
    {
        get;
        set;
    }

    public byte[]? ValueAsByteArray
    {
        get;
        set;
    }

    public uint[]? ValueAttributeArray
    {
        get;
        set;
    }

    public bool? ValueAsBool
    {
        get;
        set;
    }

    public uint? ValueAsUint
    {
        get;
        set;
    }

    public string? ValueAsDateTime
    {
        get;
        set;
    }

    public string? ValueAsString
    {
        get;
        set;
    }

    public HighLevelAttributeValueDto()
    {
        
    }
}
