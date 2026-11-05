# BouncyHsm Aspire Hosting

The BouncyHsm Nuget package for Aspire Hosting provides easy integration of the BouncyHsm system into applications built on [Aspire](https://aspire.dev/). It allows developers to declaratively create and manage BouncyHsm instances as native Aspire resources, thereby simplifying the development, testing, and operation of applications that use cryptographic services.

The Nuget BouncyHsm Aspire Hosting is built on top of the BouncyHsm Docker container.
See more <https://github.com/harrison314/BouncyHsm>.

Install package `dotnet add package BouncyHsm.Aspire.Hosting`.

Example of using BouncyHsm in an Aspire project:

```cs
var builder = DistributedApplication.CreateBuilder(args);

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

builder.Build().Run();
```