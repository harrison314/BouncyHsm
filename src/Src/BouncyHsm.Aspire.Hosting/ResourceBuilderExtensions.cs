using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Aspire.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Hosting;

public static class ResourceBuilderExtensions
{
    [AspireExport]
    public static IResourceBuilder<T> WithBouncyHsmApiRefernce<T>(this IResourceBuilder<T> builder, IResourceBuilder<BouncyHsmResource> bouncyHsm)
        where T : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(bouncyHsm);

        return builder.WithReference(bouncyHsm.GetEndpoint(EndpointName.Http));
    }

    [AspireExport]
    public static IResourceBuilder<T> WithBouncyHsmNativeLibEndpointRefernce<T>(this IResourceBuilder<T> builder,
        IResourceBuilder<BouncyHsmResource> bouncyHsm,
        BouncyHsmLogTarget? logTarget = null,
        BouncyHsmLogLevel? logLevel = null,
        string? tag = null)
        where T : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(bouncyHsm);
        if (!string.IsNullOrEmpty(tag))
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(tag.Length, 30, nameof(tag));
        }

        return builder.WithEnvironment("BOUNCY_HSM_CFG_STRING", () =>
        {
            return CreateConfiguration(bouncyHsm,
                logTarget,
                logLevel,
                tag);
        });
    }

    private static string CreateConfiguration(IResourceBuilder<BouncyHsmResource> bouncyHsm,
        BouncyHsmLogTarget? logTarget,
        BouncyHsmLogLevel? logLevel,
        string? tag)
    {
        EndpointReference endpoint = bouncyHsm.GetEndpoint(EndpointName.Tcp);
        StringBuilder sb = new StringBuilder();
        sb.Append("Server=");
        sb.Append(endpoint.Host);
        sb.Append("; ");

        sb.Append("Port=");
        sb.Append(endpoint.Port);
        sb.Append("; ");

        if (logTarget.HasValue)
        {
            sb.Append("LogTarget=");
            sb.Append(logTarget.ToString());
            sb.Append("; ");
        }

        if (logLevel.HasValue)
        {
            sb.Append("LogLevel=");
            sb.Append(logLevel.ToString());
            sb.Append("; ");
        }

        if (!string.IsNullOrEmpty(tag))
        {
            sb.Append("Tag=");
            sb.Append(tag);
            sb.Append("; ");
        }

        if (sb.Length > 0)
        {
            sb.Length--;
        }

        return sb.ToString();
    }
}
