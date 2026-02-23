using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Contracts;

public class SupportedKeys
{
    public required IReadOnlyList<String> RsaKeys
    {
        get;
        init;
    }

    public required IReadOnlyList<SupportedNameCurve> EcCurves
    {
        get;
        init;
    }

    public required IReadOnlyList<SupportedNameCurve> EdwardsCurves
    {
        get;
        init;
    }

    public required IReadOnlyList<SupportedNameCurve> MontgomeryCurves
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

    public SupportedKeys()
    {

    }
}