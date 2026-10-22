using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal class NullIterationVariablePrfDataParam : IPrfDataParam
{
    public CK_PRF_DATA_TYPE Type
    {
        get => CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE;
    }

    public NullIterationVariablePrfDataParam()
    {
    }

    public void Apply(IMac prfFunction, ref PrfDataContext context)
    {
        byte[] iteration = context.AlternativeIteration
            ?? throw new InvalidOperationException("The iteration variable is not available.");

        prfFunction.BlockUpdate(iteration, 0, iteration.Length);
    }

    public override string ToString()
    {
        return $"Prf data: {this.Type} with null";
    }
}