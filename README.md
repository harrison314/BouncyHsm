# Bouncy Hsm
![GitHub release (with filter)](https://img.shields.io/github/v/release/harrison314/BouncyHsm)
[![NuGet Status](http://img.shields.io/nuget/v/BouncyHsm.Client.svg?style=flat)](https://www.nuget.org/packages/BouncyHsm.Client/)
![GitHub License](https://img.shields.io/github/license/harrison314/BouncyHsm)

_Bouncy Hsm_ is an developer friendly implementation of a cryptographic store accessible through a PKCS#11 interface.
It can simulate HSM (hardware security module) and smart cards (also with a qualified area), 
it also includes a web administration interface and a REST interface.

_Bouncy Hsm_ was created to facilitate the development and testing of applications using PKCS#11 devices.
It is not intended for production data, as it does not implement any data and key protection in storage
or during network calls.

The _BouncyHsm_ project was created as an alternative to SoftHSMv2, due to ongoing problems I had using it.

![Screenshots - BouncyHsm web UI](Doc/Screenshots.gif)

(Screenshots from version 1.1.0)

## Features
_Bouncy Hsm_ implementig PKCS#11 interface from standard version 2.40 with some mechanism from version 3.0.

* Multiple application and users access using PKCS#11 interface.
* Slot and crypto object management using web interface and REST API.
  * Create/remove slots.
  * Plug and unplug devices (tokens).
  * Edit object attributes.
  * Import P12/PFX files.
  * Import crypto objects in PEM format.
  * Generate CSR, generate self-signed certificate, import certificate from file.
  * Generate keys.
  * ...
* Possibility to simulate cards with a qualified area and a signature PIN.
* Possibility to simulate _protected authentication path_ using web interface.
* Possibility to simulate removable devices (tokens).
* Supports RSA keys (with size 2-6K).
* Supports [80 named elliptic curves](/Doc/SuportedAlgorithms.md#elliptic-curves) and user-defined elliptic curves.
* Edwards curves (Ed25519, Ed448).
* Mongomery curves (X25519, X448).
* Supports secrets (HMAC, derive,...)
* Supports AES keys.
* Supports Salsa20 keys.
* Supports ChaCha20 keys (also ChaCha20Poly1305 algorithm).
* [Supports mechanisms](/Doc/SuportedAlgorithms.md)
* Supports [custom profiles for mechanisms](/Doc/Profiles.md) (To limit mechanisms to simulate a specific type of HSM or card).
* Same behavior and algorithm support across platforms and versions of Linux operating systems.
* Native PKCS#11 library without dependencies (no dependency hell, no permission configuration).
* _BouncyHsm_ runs on all platform supported [.Net 8.0](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md). Moreover, it can be run as a _Windows service_ and also works on  _Raspberry Pi Zero 2 W_. Native lib _BouncyHsm.Pkcs11Lib_ is awaitable for Windows x86 and x64, Linux x64, RHEL like x64 ([it can also be compiled for other platforms](/Doc/BuildPkcs11Lib.md)).
* CLI tool for management.
* Nuget ([BouncyHsm.Client](https://www.nuget.org/packages/BouncyHsm.Client)) with REST API client and native PKCS#11 libraries for unit testing. (See [example project](/Examples/BouncyHsmTestExample).)

## Quick start and deployment  guide
* [Quick start guide](/Doc/QuickstartGuide.md)
* [Deployment guide](/Doc/Deployment.md)
* [Docker support](/Doc/DockerSupport.md)

## Contributing and issues
Pull requests are welcome. If you are not sure about the change, open an issue first.

If the found error or changes refer to the PKCS#11 standard, please complete the link section of the standard.

See more rules in [CONTRIBUTING](/.github/CONTRIBUTING.md).

## Links

### Technology
* [Dotnet 8.0](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
* [BouncyCastle](https://github.com/bcgit/bc-csharp)
* [Pkcs11Interop](https://github.com/Pkcs11Interop)
* [LiteDb](https://www.litedb.org/)
* [Blazor WebAssembly](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
* [cmp](https://github.com/camgunz/cmp)
* [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)

### Other links
* [PKCS #11 Cryptographic Token Interface Base Specification Version 2.40](https://docs.oasis-open.org/pkcs11/pkcs11-curr/v2.40/os/pkcs11-curr-v2.40-os.pdf)
* [PKCS #11 Cryptographic Token Interface Base Specification Version 3.0](https://docs.oasis-open.org/pkcs11/pkcs11-curr/v3.0/pkcs11-curr-v3.0.pdf)
* [Software Ideas Modeler](https://www.softwareideas.net/) - tool in which the diagrams in the documentation were drawn
* [NSwag studio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio) - tool for generate OpenApi client
* [Ako som robil BouncyHsm](https://harrison314.github.io/BouncyHsm.html) - My blog post about BouncyHsm development, technological decisions and reasons for development - in Slovak language
