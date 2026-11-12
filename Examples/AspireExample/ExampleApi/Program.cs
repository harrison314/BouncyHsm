
using ExampleApi.Services;
using Scalar.AspNetCore;

namespace ExampleApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton<IHsmService, HsmService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapGet("/slots", (IHsmService hsmService) =>
        {
            return hsmService.GetSlots();
        })
        .WithName("GetSlotsWithToken");

        app.MapGet("/", () =>
        {
            return Results.Redirect("/scalar");
        });

        app.Run();
    }
}
