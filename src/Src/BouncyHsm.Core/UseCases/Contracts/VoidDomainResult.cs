namespace BouncyHsm.Core.UseCases.Contracts;

[Dunet.Union]
public partial record VoidDomainResult
{
    public partial record Ok();
    public partial record NotFound();
    public partial record InvalidInput(string Message);
}
