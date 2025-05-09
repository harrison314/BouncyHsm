using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;

namespace BouncyHsm.DocGenerator;

public class DocModel
{
    public List<MechanismInfo> Mechanisms
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

    public List<SupportedNameCurve> Edwards
    {
        get;
        internal set;
    }

    public int EdwardsCount
    {
        get => this.Edwards.Select(t => t.Oid).Distinct().Count();
    }

    public BouncyHsmVersion Versions
    {
        get;
        internal set;
    }

    public DocModel()
    {
        this.Mechanisms = new List<MechanismInfo>();
        this.Ec = new List<SupportedNameCurve>();
        this.Edwards = new List<SupportedNameCurve>();
        this.Versions = new BouncyHsmVersion("", "", "", "");
    }
}