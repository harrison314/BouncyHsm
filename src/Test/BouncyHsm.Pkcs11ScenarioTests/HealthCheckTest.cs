using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Pkcs11ScenarioTests;

[TestClass]
public sealed class HealthCheckTest
{
    [TestMethod]
    public async Task HealthCheck_Call_Resturn200()
    {
        string healtchekEndpoint = string.Concat(BchClient.BouncyHsmEndpoint.TrimEnd('/'), "/health");
        using HttpResponseMessage response = await BchClient.httpClient.GetAsync(healtchekEndpoint);
        response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadAsStringAsync();
    }
}
