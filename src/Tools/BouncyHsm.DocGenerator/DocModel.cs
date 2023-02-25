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

    public List<SupportedNameCurve> Ec
    {
        get;
        internal set;
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