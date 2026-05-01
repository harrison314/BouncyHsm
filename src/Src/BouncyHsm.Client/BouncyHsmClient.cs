namespace BouncyHsm.Client;

public partial class BouncyHsmClient
{
    public BouncyHsmClient(string baseAddress, HttpClient httpClient)
        : this(httpClient)
    {
        if (baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

        this._baseUrl = baseAddress;
    }
}
