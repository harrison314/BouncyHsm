// SET ASPIRE_CONTAINER_RUNTIME=wslc

using AspireExample.AppHost;
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

string aesKeyPem = """
    -----BEGIN AES SECRET KEY-----
    W4lThm/Ii+bRe5LPUsWiND6uHNYaobZKHJr0QC9au+o=
    -----END AES SECRET KEY-----
    """;

var bouncyHsm = builder.AddBouncyHsm("bouncyhsm")
    .WithStorage(BouncyHsmStorage.InMemory) // InMemory or LiteDbInVolume with mount volume
    .WithHsmSlotAndToken("Slot1", "oper1", "123456", "12345678") // crate a new slot after start
        .WithImportPkcs12("oper1",
            TestData.GenerateP12(), "password",
            Encoding.UTF8.GetBytes("SigningCertificate"), "SigningCertificate") // import P12 file as private key, public key and certificate
    .WithHsmSlotAndToken("Slot2", "oper2", "123456", "12345678")
        .WithImportPemObjects("oper2",
            aesKeyPem,
            Encoding.UTF8.GetBytes("AES-key"), "AES-key",
            BouncyHsmPemHint.ForEncryption | BouncyHsmPemHint.ForWrap); // import PEM objects

builder.AddProject<Projects.ExampleApi>("exampleapi")
     .WaitForStart(bouncyHsm)
     .WithBouncyHsmApiReference(bouncyHsm) // reference HTTP endpoint for BouncyHSM API
     .WithBouncyHsmNativeLibEndpointReference(bouncyHsm, BouncyHsmLogTarget.NONE, BouncyHsmLogLevel.NONE, tag: "exampleapi"); // reference environment variable from PKCS#11 native library

#endif

builder.Build().Run();
