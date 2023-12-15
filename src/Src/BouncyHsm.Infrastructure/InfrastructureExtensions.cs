using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BouncyHsm.Infrastructure;

public static class InfrastructureExtensions
{
    public static void RegisterCommonServices(this IServiceCollection serviceDescriptors)
    {
        serviceDescriptors.AddTransient<IProtectedAuthPathProvider, NullProtectedAuthPathProvider>();
        serviceDescriptors.AddSingleton<ITimeAccessor, BouncyHsm.Infrastructure.Common.TimeAccessor>();
        serviceDescriptors.AddTransient<IP11HwServices, BouncyHsm.Infrastructure.Common.P11HwServices>();
    }

    public static IServiceCollection RegisterInMemoryPersistence(this IServiceCollection serviceDescriptors)
    {
        serviceDescriptors.AddSingleton<IPersistentRepository, BouncyHsm.Infrastructure.Storage.InMemory.MemoryPersistentRepository>();

        return serviceDescriptors;
    }

    public static IServiceCollection RegisterLiteDbPersistence(this IServiceCollection serviceDescriptors, IConfiguration configuration)
    {
        serviceDescriptors.Configure<BouncyHsm.Infrastructure.Storage.LiteDbFile.LiteDbPersistentRepositorySetup>(configuration.GetSection("LiteDbPersistentRepositorySetup"));
        serviceDescriptors.AddSingleton<IPersistentRepository, BouncyHsm.Infrastructure.Storage.LiteDbFile.LiteDbPersistentRepository>();

        return serviceDescriptors;
    }

    public static IServiceCollection RegisterInMemoryClientAppCtx(this IServiceCollection serviceDescriptors)
    {
        serviceDescriptors.AddSingleton<IClientApplicationContext, BouncyHsm.Infrastructure.Cap.InMemory.ClientApplicationContext>();

        return serviceDescriptors;
    }
}