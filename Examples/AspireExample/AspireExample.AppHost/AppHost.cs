// SET ASPIRE_CONTAINER_RUNTIME=wslc

using AspireExample.AppHost;
using BouncyHsm.Aspire.Hosting;
using System.Text;

var builder = DistributedApplication.CreateBuilder(args);

#if false
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

#else

var bouncyHsm = builder.AddBouncyHsm("bouncyhsm")
    .WithStorage(BouncyHsmStorage.InMemory)
    .WithHsmSlotAndToken("Slot1", "oper1", "123456", "12345678")
        .WithImportPkcs12("oper1",
            TestData.GenerateP12(), "password",
            Encoding.UTF8.GetBytes("SigningCertificate"), "SigningCertificate")
    .WithHsmSlotAndToken("Slot2", "oper2", "123456", "12345678");

builder.AddProject<Projects.ExampleApi>("exampleapi")
     .WaitForStart(bouncyHsm)
     .WithBouncyHsmApiReference(bouncyHsm)
     .WithBouncyHsmNativeLibEndpointReference(bouncyHsm, BouncyHsmLogTarget.NONE, BouncyHsmLogLevel.NONE, tag: "exampleapi");

#endif

builder.Build().Run();
