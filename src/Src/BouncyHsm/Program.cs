
using BouncyHsm.Infrastructure;
using BouncyHsm.Infrastructure.Application;
using BouncyHsm.Services.Configuration;
using Serilog;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BouncyHsm;

public class Program
{
    public static void Main(string[] args)
    {
        //Log.Logger = new LoggerConfiguration()
        //    .MinimumLevel.Verbose()
        //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        //.Enrich.FromLogContext()
        //.WriteTo.Console()
        //.CreateBootstrapLogger();

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
#if DEBUG
        builder.Host.UseDefaultServiceProvider((hotBuilder, optios) =>
        {
            optios.ValidateOnBuild = true;
        });
#endif
        builder.Host.UseSerilog((context, services, configuration) => configuration
               .ReadFrom.Configuration(context.Configuration)
               .ReadFrom.Services(services)
               .Enrich.FromLogContext()
               .WriteTo.Console());


        builder.Services.Configure<Services.Configuration.BouncyHsmSetup>(builder.Configuration.GetSection("BouncyHsmSetup"));
        // Add services to the container.

        builder.Services.AddControllers()
            .AddJsonOptions(cfg =>
            {
                cfg.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                cfg.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                cfg.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(cfg =>
        {
            cfg.SupportNonNullableReferenceTypes();
        });


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
            app.UseSwagger();
            app.UseSwaggerUI();
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