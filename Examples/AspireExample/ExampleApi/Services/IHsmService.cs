namespace ExampleApi.Services;

public interface IHsmService
{
    List<TokenInfo> GetSlots();
}
