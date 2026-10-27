using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Hosting;

public static class ResourceBuilderExtensions
{
    [AspireExport]
    public static IResourceBuilder<T> WithBouncyHsmApiRefernce<T>(this IResourceBuilder<T> builder, IResourceBuilder<BouncyHsmResource> bouncyHsm)
        where T : IResource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(bouncyHsm);

        throw new NotImplementedException();
    }

    [AspireExport]
    public static IResourceBuilder<T> WithBouncyHsmNativeLibEndpointRefernce<T>(this IResourceBuilder<T> builder,
        IResourceBuilder<BouncyHsmResource> bouncyHsm)
        where T : IResource
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(bouncyHsm);

        //TODO: log source, log level, tag
        throw new NotImplementedException();
    }
}
