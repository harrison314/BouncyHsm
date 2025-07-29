using BouncyHsm.Core.Rpc;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class InitPINHandler : IRpcRequestHandler<InitPinRequest, InitPinEnvelope>
{
    public InitPINHandler()
    {
    }

    public ValueTask<InitPinEnvelope> Handle(InitPinRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("InitTokenHandler is not implemented yet.");
    }
}