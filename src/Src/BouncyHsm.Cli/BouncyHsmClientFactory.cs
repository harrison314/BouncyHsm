using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BouncyHsm.Spa.Services.Client;

namespace BouncyHsm.Cli;

internal static class BouncyHsmClientFactory
{
    private readonly static HttpClient httpClient = new HttpClient();

    public static BouncyHsmClient Create(string endpoint)
    {
        return new BouncyHsmClient(endpoint, httpClient);
    }
}
