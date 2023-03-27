
using BouncyHsm.Infrastructure;
using BouncyHsm.Infrastructure.Application;
using BouncyHsm.Services.Configuration;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BouncyHsm;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationOptions webApplicationOptions = new WebApplicationOptions()
        {
            Args = args,
            ContentRootPath = WindowsServiceHelpers.IsWindowsService()
                                     ? AppContext.BaseDirectory : default
        };

        WebApplicationBuilder builder = WebApplication.CreateBuilder(webApplicationOptions);
#if DEBUG
        builder.Host.UseDefaultServiceProvider((hotBuilder, options) =>
        {
            options.ValidateOnBuild = true;
        });
#endif
        builder.Host.UseSerilog((context, services, configuration) => configuration
               .ReadFrom.Configuration(context.Configuration)
               .ReadFrom.Services(services)
               .Enrich.FromLogContext());

        builder.Services.Configure<Services.Configuration.BouncyHsmSetup>(builder.Configuration.GetSection("BouncyHsmSetup"));
        // Add services to the container.

        builder.Services.AddScoped<BouncyHsm.Infrastructure.Filters.HttpResponseExceptionFilter>();
        builder.Services.AddControllers(cfg =>
        {
            cfg.Filters.Add<BouncyHsm.Infrastructure.Filters.HttpResponseExceptionFilter>();
        })
            .AddJsonOptions(cfg =>
            {
                cfg.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                cfg.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                cfg.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(cfg =>
        {
            cfg.Title = "Bouncy Hsm REST API";
            cfg.Description = "Management API for Bouncy Hsm.";
            cfg.UseRouteNameAsOperationId = true;
        });

        builder.Host.UseWindowsService();

        builder.Services.AddP11Handlers();
        builder.Services.AddHostedService<Infrastructure.HostedServices.TcpHostedService>();

        builder.Services.RegisterCommonServices();
        builder.Services.RegisterInMemoryClientAppCtx();

        _ = builder.Configuration["PersistenceStorageType"] switch
        {
            "InMemory" => builder.Services.RegisterInMemoryPersistence(),
            "LiteDb" => builder.Services.RegisterLiteDbPersistence(builder.Configuration),
            _ => throw new InvalidDataException($"PersistenceStorageType {builder.Configuration["PersistenceStorageType"]} is not supported.")
        };


        builder.Services.AddScoped<BouncyHsm.Core.UseCases.Contracts.ISlotFacade, BouncyHsm.Core.UseCases.Implementation.SlotFacade>();
        builder.Services.AddScoped<BouncyHsm.Core.UseCases.Contracts.IHsmInfoFacade, BouncyHsm.Core.UseCases.Implementation.HsmInfoFacade>();
        builder.Services.AddScoped<BouncyHsm.Core.UseCases.Contracts.IStorageObjectsFacade, BouncyHsm.Core.UseCases.Implementation.StorageObjectsFacade>();
        builder.Services.AddScoped<BouncyHsm.Core.UseCases.Contracts.IPkcsFacade, BouncyHsm.Core.UseCases.Implementation.PkcsFacade>();
        builder.Services.AddScoped<BouncyHsm.Core.UseCases.Contracts.IStatsFacade, BouncyHsm.Core.UseCases.Implementation.StatsFacade>();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
        });

        WebApplication app = builder.Build();

        app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }

        if (app.Environment.IsDevelopment()
            || app.Configuration.GetValue<bool>($"{nameof(BouncyHsmSetup)}:{nameof(BouncyHsmSetup.EnableSwagger)}"))
        {
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }

        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthorization();


        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}