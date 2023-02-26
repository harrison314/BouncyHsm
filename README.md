# Bouncy Hsm

**Work in progress** 

_Bouncy Hsm_ is an developr frendly implementation of a cryptographic store accessible through a PKCS#11 interface.
It can simulate HSM (hardware security module) and smartcards (also with a qualified area), 
it also includes a web administration interface and a REST interface.

_Bouncy Hsm_ was created to facilitate the development and testing of applications using PKCS#11 devices.
It is not intended for production data, as it does not implement any data and key protection in storage
or during network calls.

## Features
* Multiple application access using PKCS#11 interface.
* Slot and crypto object managment using web interface and rest api.
  * Create/remove slots.
  * Import P12/PFX files.
  * ...
* Possibility to simulate cards with a qualified area and a signature pin.
* Suports RSA keys (with size 1-10K).
* Suports 80 named eliptic curves.
* Suports secrets (HMAC, serive,...)
* [Suports mechanisms](/Doc/SuportedAlgorithms.md) **TODO**
* Same behavior and algorithm support across platforms and versions of Linux operating systems.
* Native PKCS#11 library without dependencies (no dependency hell, no permision configuration).
* _BouncyHsm_ runs on _Raspberry Pi Zero 2 W_.

## Technology
* [Dotnet 6.0](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6)
* [BouncyCastle](https://github.com/bcgit/bc-csharp)
* [Pkcs11Interop](https://github.com/Pkcs11Interop)
* [LiteDb](https://www.litedb.org/)
* Blazor WebAssembly
* [cmp](https://github.com/camgunz/cmp)
* [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)

