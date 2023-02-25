using BouncyHsm.Spa;
using BouncyHsm.Spa.Services.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BouncyHsm.Spa
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<IBouncyHsmClient>(sp =>
            {
                return new BouncyHsmClient(builder.HostEnvironment.BaseAddress,
                    sp.GetRequiredService<HttpClient>());
            });

            await builder.Build().RunAsync();
        }
    }
}