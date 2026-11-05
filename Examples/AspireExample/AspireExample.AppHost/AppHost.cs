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
    .WithStorage(storage: BouncyHsmStorage.InMemory) // InMemory or LiteDbInVolume with mount volume
    .WithHsmSlotAndToken(slotDescription: "Slot1", tokenLabel: "oper1", userPin: "123456", soPin: "12345678") // crate a new slot after start
        .WithImportPkcs12(tokenLabel: "oper1",
            pkcs12Content: TestData.GenerateP12(), password: "password",
            ckaId: Encoding.UTF8.GetBytes("SigningCertificate"), ckaLabel: "SigningCertificate") // import P12 file as private key, public key and certificate
    .WithHsmSlotAndToken(slotDescription: "Slot2", tokenLabel: "oper2", userPin: "123456", soPin: "12345678")
        .WithImportPemObjects(tokenLabel: "oper2",
            pem: aesKeyPem,
            ckaId: Encoding.UTF8.GetBytes("AES-key"), ckaLabel: "AES-key",
            hints: BouncyHsmPemHint.ForEncryption | BouncyHsmPemHint.ForWrap); // import PEM objects

builder.AddProject<Projects.ExampleApi>("exampleapi")
     .WaitForStart(bouncyHsm)
     .WithBouncyHsmApiReference(bouncyHsm) // reference HTTP endpoint for BouncyHSM API
     .WithBouncyHsmNativeLibEndpointReference(bouncyHsm, BouncyHsmLogTarget.NONE, BouncyHsmLogLevel.NONE, tag: "exampleapi"); // reference environment variable from PKCS#11 native library

#endif

builder.Build().Run();
