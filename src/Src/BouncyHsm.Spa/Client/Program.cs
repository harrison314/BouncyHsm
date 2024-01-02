using BlazorStrap;
using BouncyHsm.Client;
using BouncyHsm.Spa;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BouncyHsm.Spa;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddScoped<IBouncyHsmClient>(sp =>
        {
            return new BouncyHsmClient(builder.HostEnvironment.BaseAddress,
                sp.GetRequiredService<HttpClient>());
        });

        builder.Services.AddBlazorStrap();

#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
#else
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#endif

        await builder.Build().RunAsync();
    }
}