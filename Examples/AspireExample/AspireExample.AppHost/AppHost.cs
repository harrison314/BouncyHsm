// SET ASPIRE_CONTAINER_RUNTIME=wslc

var builder = DistributedApplication.CreateBuilder(args);

var bouncyHsm = builder
    .AddContainer("bouncyhsm", "ghcr.io/harrison314/bouncyhsm:v2.3.2")
    .WithImagePullPolicy(ImagePullPolicy.Default)
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithEndpoint("tcp", e =>
    {
        e.TargetPort = 8765;
        e.Protocol = System.Net.Sockets.ProtocolType.Tcp;
    })
    .WithHttpHealthCheck("/health");


builder.AddProject<Projects.ExampleApi>("exampleapi")
     .WithReference(bouncyHsm.GetEndpoint("http")).WaitForStart(bouncyHsm)
     .WithEnvironment("BOUNCY_HSM_CFG_STRING", () =>
     {
         EndpointReference tcpEndpoint = bouncyHsm.GetEndpoint("tcp");
         return $"Server={tcpEndpoint.Host};Port={tcpEndpoint.Port}";
     });

builder.Build().Run();
