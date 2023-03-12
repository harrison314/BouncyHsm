namespace BouncyHsm.Spa.Services.Client;

internal partial class BouncyHsmClient
{
    public BouncyHsmClient(string baseAddress, HttpClient httpClient)
        : this(httpClient)
    {
        this._baseUrl = baseAddress;
    }
}