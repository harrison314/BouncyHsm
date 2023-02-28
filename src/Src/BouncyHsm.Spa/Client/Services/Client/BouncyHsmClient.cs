namespace BouncyHsm.Spa.Services.Client;

internal partial class BouncyHsmClient
{
    public BouncyHsmClient(string baseAdress, HttpClient httpClient)
        : this(httpClient)
    {
        this._baseUrl = baseAdress;
    }
}