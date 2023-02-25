namespace BouncyHsm.Core.UseCases.Contracts;

[Dunet.Union]
public partial record DomainResult<T>
{
    public partial record Ok(T Value);
    public partial record NotFound();
    public partial record InvalidInput(string Message);

}
