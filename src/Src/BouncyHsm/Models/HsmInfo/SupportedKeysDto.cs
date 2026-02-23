
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Models.HsmInfo;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.SupportedKeys))]
public class SupportedKeysDto
{
    public required IReadOnlyList<String> RsaKeys
    {
        get;
        init;
    }

    public required IReadOnlyList<CurveInfoDto> EcCurves
    {
        get;
        init;
    }

    public required IReadOnlyList<CurveInfoDto> EdwardsCurves
    {
        get;
        init;
    }

    public required IReadOnlyList<CurveInfoDto> MontgomeryCurves
    {
        get;
        init;
    }
    public required IReadOnlyList<String> MlDsaKeys
    {
        get;
        init;
    }

    public required IReadOnlyList<String> SlhDsaKeys
    {
        get;
        init;
    }

    public required IReadOnlyList<String> MlKemKeys
    {
        get;
        init;
    }

    public SupportedKeysDto()
    {

    }
}