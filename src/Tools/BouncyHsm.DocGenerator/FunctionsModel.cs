using BouncyHsm.Core.UseCases.Contracts;

namespace BouncyHsm.DocGenerator;

public class FunctionsModel
{
    public BouncyHsmVersion Versions
    {
        get;
        internal set;
    }
    public List<FunctionInfo> Functions
    {
        get;
        internal set;
    }

    public FunctionsModel()
    {
        this.Functions = new List<FunctionInfo>();
        this.Versions = new BouncyHsmVersion("", "", [], "");
    }
}