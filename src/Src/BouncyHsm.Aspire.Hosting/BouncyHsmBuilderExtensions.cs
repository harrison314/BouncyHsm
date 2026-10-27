using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Aspire.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Hosting;

public static class BouncyHsmBuilderExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> AddBouncyHsm(this IDistributedApplicationBuilder builder, [ResourceName] string name, BouncyHsmImage? image = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (image == null)
        {
            image = BouncyHsmImage.Current;
        }

        BouncyHsmResource bouncyHsmResource = new BouncyHsmResource(name);
        IResourceBuilder<BouncyHsmResource> rsBuilder = builder.AddResource(bouncyHsmResource)
             .WithImage(image.Image, image.Tag)
             .WithImagePullPolicy(ImagePullPolicy.Default)
                 .WithHttpEndpoint(targetPort: 8080, name: EndpointName.Http)
                 .WithEndpoint(EndpointName.Tcp, e =>
                 {
                     e.TargetPort = 8765;
                     e.Protocol = System.Net.Sockets.ProtocolType.Tcp;
                 })
                 .WithHttpHealthCheck("/health")
                 .OnResourceReady((resource, readyEvenet, ct) =>
                 {
                     //TODO: use after initialize tasks
                     return Task.CompletedTask;
                 });


        return rsBuilder;
    }

    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithStorage(this IResourceBuilder<BouncyHsmResource> builder, BouncyHsmStorage storage, string? mapVolumePath = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        string storageValue = storage switch
        {
            BouncyHsmStorage.InMemory => "InMemory",
            BouncyHsmStorage.LiteDbInVolume => "LiteDb",
            _ => throw new NotImplementedException($"Enum value {storage} is not supported.")
        };

        builder = builder.WithEnvironment("BouncyHsm_PersistenceStorageType", storageValue);

        if (!string.IsNullOrEmpty(mapVolumePath))
        {
            builder = builder.WithBindMount(mapVolumePath, "/var/BouncyHsm/");
        }

        return builder;
    }
}
