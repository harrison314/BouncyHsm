using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;

namespace BouncyHsm.DocGenerator;

public class DocModel
{
    public List<MechanismInfoData> Mechanisms
    {
        get;
        internal set;
    }

    public int MechanismsCount
    {
        get => this.Mechanisms.Count;
    }

    public List<SupportedNameCurve> Ec
    {
        get;
        internal set;
    }

    public int EcCount
    {
        get => this.Ec.Select(t => t.Oid).Distinct().Count();
    }

    public BouncyHsmVersion Versions
    {
        get;
        internal set;
    }

    public DocModel()
    {
        this.Mechanisms = new List<MechanismInfoData>();
        this.Ec = new List<SupportedNameCurve>();
        this.Versions = new BouncyHsmVersion("", "", "", "");
    }
}