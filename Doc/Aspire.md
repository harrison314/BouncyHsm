# Bouncy Hsm Aspire Hosting

The [Bouncy Hsm Nuget package for Aspire](https://www.nuget.org/packages/BouncyHsm.Aspire.Hosting/) (v. 17) provides easy integration of the BouncyHsm system into applications built on [Aspire](https://aspire.dev/). It allows developers to declaratively create and manage BouncyHsm instances as native Aspire resources, thereby simplifying the development, testing, and operation of applications that use cryptographic services.

This package allows you to configure the _Bouncy Hsm_ environment at startup and create new tokens (and slots) within it, and then import cryptographic objects in PEM and P12/PFX formats into them.

The Nuget Bouncy Hsm Aspire Hosting is built on top of the BouncyHsm Docker container.
See more <https://github.com/harrison314/BouncyHsm>.

Install package `dotnet add package BouncyHsm.Aspire.Hosting`.

Example of using _Bouncy Hsm_ in an Aspire project:

```cs
var builder = DistributedApplication.CreateBuilder(args);

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

builder.AddDotnetProject("exampleapi", "../ExampleApi/ExampleApi.csproj")
     .WaitForStart(bouncyHsm)
     .WithBouncyHsmApiReference(bouncyHsm) // reference HTTP endpoint for BouncyHSM API
     .WithBouncyHsmNativeLibEndpointReference(bouncyHsm, BouncyHsmLogTarget.NONE, BouncyHsmLogLevel.NONE, tag: "exampleapi"); // reference environment variable from PKCS#11 native library

builder.Build().Run();
```

A complete demonstration of Aspire usage can be found at <https://github.com/harrison314/BouncyHsm/tree/main/Examples/AspireExample>.