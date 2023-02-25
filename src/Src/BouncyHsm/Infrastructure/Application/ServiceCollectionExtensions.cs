using BouncyHsm.Core.Services.P11Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Application;

internal static class ServiceCollectionExtensions
{
    public static void AddP11Handlers(this IServiceCollection services)
    {
        foreach ((Type tInterface, Type tClass) in RpcDefinitionUtils.GetRegistrations())
        {
            services.AddTransient(tInterface, tClass);
        }
    }
}
