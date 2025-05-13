using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Text;
using PkcsExtensions;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography.X509Certificates;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T18_CreateObject
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void CreateObject_DataObject_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);


        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, "MyObject"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Encoding.UTF8.GetBytes("Hello wold!")),
        };

        _ = session.CreateObject(objectAttributes);
    }

    [TestMethod]
    public void CreateObject_TokenDataObject_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);


        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"MyObjectToken-{DateTime.UtcNow}-{Random.Shared.Next(100,999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Encoding.UTF8.GetBytes("Hello wold!")),
        };

        _ = session.CreateObject(objectAttributes);
    }

    [TestMethod]
    public void CreateObject_X509Certificate_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"CertTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] certificate = Convert.FromBase64String(@"MIIDaDCCAlCgAwIBAgIUHy7xGvpt1K/QwF/t5Un+95MtJjIwDQYJKoZIhvcNAQEL
BQAwNzELMAkGA1UEBhMCU0sxEzARBgNVBAgMCkJyYXRpc2xhdmExEzARBgNVBAcM
CkJyYXRpc2xhdmEwIBcNMjMwMjAxMTgzMjMxWhgPMjUyMjEwMDMxODMyMzFaMDcx
CzAJBgNVBAYTAlNLMRMwEQYDVQQIDApCcmF0aXNsYXZhMRMwEQYDVQQHDApCcmF0
aXNsYXZhMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAoVoLqut5sWpS
hhHmt2TeTS2ccpuGd/6nHdE/7VFeenplLW1gu6aBi44OGutJvsrzcWFT9vimh6+f
e+FjpbObFX/5sG/uT7JiDNhBSNw168hmxNdPFnF+Ry8t1TswOWF+XncLe4iQENLL
HnVP1flqL2aWxpeiA5mNedEvphttN14nB2yPdWoY2PWITEneK8UwhitGtYYw6q35
fwrTLzWDT4/C5LZOU1NLTGlnkxtdHcqc7mZHLcnRtwjeCNtsLSo6niHszMzf0php
yhk2+E6hMF9qX2Tixl7KAlRWShmYlg5Nt6p9U6reljYxYf4Hvz+w/+Z3h9EBqfIr
75b/pW4XOQIDAQABo2owaDAdBgNVHQ4EFgQU7d4qcKN96aH2lO/b9l7A8QkNRS0w
HwYDVR0jBBgwFoAU7d4qcKN96aH2lO/b9l7A8QkNRS0wDgYDVR0PAQH/BAQDAgXg
MBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMDMA0GCSqGSIb3DQEBCwUAA4IBAQCGPKFY
TZeC3gBiyIvsqYvevNk14vXD/6/PnB4FRHIBBUgf3nsHytuTelNpWpdlucB/0NUx
QEt8Q1G3M5RQ7EPZo0Hv9zykS/IqQHLQw9nFTWK2LmYDZrtxJzH3bwN4Y8bI5xrh
RvApqJ41cpgDjOQ5O+vxlv4Hp+QycTFvk4xI4yfeUXxCnVxtPcFxyz70ncz0F4Dt
Xc1k0B102a2IR07vzB+znmBjbyLfdkM7Z2qT1y7pa5g0W2DpEJ81lx1NsbPwTPch
KiigHtOePBm6elKt0VC0OwS263AWc5mluobL0HXdORSyAPULuEopt597XVbFQ0Jv
TnCoPhVFsVeDjQwg");
        byte[] subject = HexConvertor.GetBytes("3037310B300906035504061302534B3113301106035504080C0A4272617469736C6176613113301106035504070C0A4272617469736C617661");
        byte[] issuer = HexConvertor.GetBytes("3037310B300906035504061302534B3113301106035504080C0A4272617469736C6176613113301106035504070C0A4272617469736C617661");
        byte[] serialNumber = HexConvertor.GetBytes("02141F2EF11AFA6DD4AFD0C05FEDE549FEF7932D2632");

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TRUSTED, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SUBJECT, subject),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ISSUER, issuer),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SERIAL_NUMBER, serialNumber),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, certificate),

            factories.ObjectAttributeFactory.Create(CKA.CKA_START_DATE, new DateTime(2012,12,1)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_END_DATE, new DateTime(2013,6,20))

        };

        _ = session.CreateObject(objectAttributes);
    }


    [TestMethod]
    public void CreateObject_X509CertificateWithHashePsk_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"CertTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] certificate = Convert.FromBase64String(@"MIIDaDCCAlCgAwIBAgIUHy7xGvpt1K/QwF/t5Un+95MtJjIwDQYJKoZIhvcNAQEL
BQAwNzELMAkGA1UEBhMCU0sxEzARBgNVBAgMCkJyYXRpc2xhdmExEzARBgNVBAcM
CkJyYXRpc2xhdmEwIBcNMjMwMjAxMTgzMjMxWhgPMjUyMjEwMDMxODMyMzFaMDcx
CzAJBgNVBAYTAlNLMRMwEQYDVQQIDApCcmF0aXNsYXZhMRMwEQYDVQQHDApCcmF0
aXNsYXZhMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAoVoLqut5sWpS
hhHmt2TeTS2ccpuGd/6nHdE/7VFeenplLW1gu6aBi44OGutJvsrzcWFT9vimh6+f
e+FjpbObFX/5sG/uT7JiDNhBSNw168hmxNdPFnF+Ry8t1TswOWF+XncLe4iQENLL
HnVP1flqL2aWxpeiA5mNedEvphttN14nB2yPdWoY2PWITEneK8UwhitGtYYw6q35
fwrTLzWDT4/C5LZOU1NLTGlnkxtdHcqc7mZHLcnRtwjeCNtsLSo6niHszMzf0php
yhk2+E6hMF9qX2Tixl7KAlRWShmYlg5Nt6p9U6reljYxYf4Hvz+w/+Z3h9EBqfIr
75b/pW4XOQIDAQABo2owaDAdBgNVHQ4EFgQU7d4qcKN96aH2lO/b9l7A8QkNRS0w
HwYDVR0jBBgwFoAU7d4qcKN96aH2lO/b9l7A8QkNRS0wDgYDVR0PAQH/BAQDAgXg
MBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMDMA0GCSqGSIb3DQEBCwUAA4IBAQCGPKFY
TZeC3gBiyIvsqYvevNk14vXD/6/PnB4FRHIBBUgf3nsHytuTelNpWpdlucB/0NUx
QEt8Q1G3M5RQ7EPZo0Hv9zykS/IqQHLQw9nFTWK2LmYDZrtxJzH3bwN4Y8bI5xrh
RvApqJ41cpgDjOQ5O+vxlv4Hp+QycTFvk4xI4yfeUXxCnVxtPcFxyz70ncz0F4Dt
Xc1k0B102a2IR07vzB+znmBjbyLfdkM7Z2qT1y7pa5g0W2DpEJ81lx1NsbPwTPch
KiigHtOePBm6elKt0VC0OwS263AWc5mluobL0HXdORSyAPULuEopt597XVbFQ0Jv
TnCoPhVFsVeDjQwg");
        byte[] subject = HexConvertor.GetBytes("3037310B300906035504061302534B3113301106035504080C0A4272617469736C6176613113301106035504070C0A4272617469736C617661");
        byte[] issuer = HexConvertor.GetBytes("3037310B300906035504061302534B3113301106035504080C0A4272617469736C6176613113301106035504070C0A4272617469736C617661");
        byte[] serialNumber = HexConvertor.GetBytes("02141F2EF11AFA6DD4AFD0C05FEDE549FEF7932D2632");
        byte[] hashOfPsk = HexConvertor.GetBytes("0b90c6bb09f449342bcabf6666636e919cad544d");

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TRUSTED, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SUBJECT, subject),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ISSUER, issuer),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SERIAL_NUMBER, serialNumber),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, certificate),

            factories.ObjectAttributeFactory.Create(CKA.CKA_NAME_HASH_ALGORITHM, (uint) CKM.CKM_SHA_1),
            factories.ObjectAttributeFactory.Create(CKA.CKA_HASH_OF_SUBJECT_PUBLIC_KEY, hashOfPsk),

            factories.ObjectAttributeFactory.Create(CKA.CKA_START_DATE, new DateTime(2012,12,1)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_END_DATE, new DateTime(2013,6,20))

        };

        _ = session.CreateObject(objectAttributes);
    }

    [TestMethod]
    public void CreateObject_X509CertificateWithHasheIssuer_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"CertTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] certificate = Convert.FromBase64String(@"MIIDaDCCAlCgAwIBAgIUHy7xGvpt1K/QwF/t5Un+95MtJjIwDQYJKoZIhvcNAQEL
BQAwNzELMAkGA1UEBhMCU0sxEzARBgNVBAgMCkJyYXRpc2xhdmExEzARBgNVBAcM
CkJyYXRpc2xhdmEwIBcNMjMwMjAxMTgzMjMxWhgPMjUyMjEwMDMxODMyMzFaMDcx
CzAJBgNVBAYTAlNLMRMwEQYDVQQIDApCcmF0aXNsYXZhMRMwEQYDVQQHDApCcmF0
aXNsYXZhMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAoVoLqut5sWpS
hhHmt2TeTS2ccpuGd/6nHdE/7VFeenplLW1gu6aBi44OGutJvsrzcWFT9vimh6+f
e+FjpbObFX/5sG/uT7JiDNhBSNw168hmxNdPFnF+Ry8t1TswOWF+XncLe4iQENLL
HnVP1flqL2aWxpeiA5mNedEvphttN14nB2yPdWoY2PWITEneK8UwhitGtYYw6q35
fwrTLzWDT4/C5LZOU1NLTGlnkxtdHcqc7mZHLcnRtwjeCNtsLSo6niHszMzf0php
yhk2+E6hMF9qX2Tixl7KAlRWShmYlg5Nt6p9U6reljYxYf4Hvz+w/+Z3h9EBqfIr
75b/pW4XOQIDAQABo2owaDAdBgNVHQ4EFgQU7d4qcKN96aH2lO/b9l7A8QkNRS0w
HwYDVR0jBBgwFoAU7d4qcKN96aH2lO/b9l7A8QkNRS0wDgYDVR0PAQH/BAQDAgXg
MBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMDMA0GCSqGSIb3DQEBCwUAA4IBAQCGPKFY
TZeC3gBiyIvsqYvevNk14vXD/6/PnB4FRHIBBUgf3nsHytuTelNpWpdlucB/0NUx
QEt8Q1G3M5RQ7EPZo0Hv9zykS/IqQHLQw9nFTWK2LmYDZrtxJzH3bwN4Y8bI5xrh
RvApqJ41cpgDjOQ5O+vxlv4Hp+QycTFvk4xI4yfeUXxCnVxtPcFxyz70ncz0F4Dt
Xc1k0B102a2IR07vzB+znmBjbyLfdkM7Z2qT1y7pa5g0W2DpEJ81lx1NsbPwTPch
KiigHtOePBm6elKt0VC0OwS263AWc5mluobL0HXdORSyAPULuEopt597XVbFQ0Jv
TnCoPhVFsVeDjQwg");
        byte[] subject = HexConvertor.GetBytes("3037310B300906035504061302534B3113301106035504080C0A4272617469736C6176613113301106035504070C0A4272617469736C617661");
        byte[] issuer = HexConvertor.GetBytes("3037310B300906035504061302534B3113301106035504080C0A4272617469736C6176613113301106035504070C0A4272617469736C617661");
        byte[] serialNumber = HexConvertor.GetBytes("02141F2EF11AFA6DD4AFD0C05FEDE549FEF7932D2632");
        byte[] hashOfIssuer = HexConvertor.GetBytes("0b90c6bb09f449342bcabf86a6636e919cad544d");

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_CERTIFICATE_TYPE, CKC.CKC_X_509),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TRUSTED, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SUBJECT, subject),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ISSUER, issuer),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SERIAL_NUMBER, serialNumber),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, certificate),

            factories.ObjectAttributeFactory.Create(CKA.CKA_NAME_HASH_ALGORITHM, (uint) CKM.CKM_SHA_1),
            factories.ObjectAttributeFactory.Create(CKA.CKA_HASH_OF_ISSUER_PUBLIC_KEY, hashOfIssuer),

            factories.ObjectAttributeFactory.Create(CKA.CKA_START_DATE, new DateTime(2012,12,1)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_END_DATE, new DateTime(2013,6,20))

        };

        _ = session.CreateObject(objectAttributes);
    }

    [DataTestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, 5)]
    [DataRow(CKK.CKK_GENERIC_SECRET, 512)]
    [DataRow(CKK.CKK_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, 64)]
    public void CreateObject_GeneralSeecrit_Success(CKK type, int size)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] secret = new byte[size];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, type),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        _ = session.CreateObject(objectAttributes);
    }

    [DataTestMethod]
    [DataRow(16)]
    [DataRow(24)]
    [DataRow(32)]
    public void CreateObject_Aes_Success(int size)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Aes-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] secret = new byte[size];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_AES),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        _ = session.CreateObject(objectAttributes);
    }

    [TestMethod]
    public void CreateObject_EcPrivateKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Ec-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        Utils.EcdhData ecdhData = Utils.CreateEcdhParams();

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_ECDSA),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, ecdhData.X9Parameters.ToAsn1Object().GetEncoded()),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, ecdhData.EcPrivateKey.D.ToByteArrayUnsigned())
        };

        _ = session.CreateObject(privateKeyAttributes);
    }

    [TestMethod]
    public void CreateObject_Poly1305Key_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Poly1305-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] secret = new byte[32];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_POLY1305),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        IObjectHandle handle = session.CreateObject(objectAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_ChaCha20Key_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"ChaCha20-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] secret = new byte[32];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_CHACHA20),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        IObjectHandle handle = session.CreateObject(objectAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_Salsa20Key_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Salsa20-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] secret = new byte[32];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_SALSA20),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        IObjectHandle handle = session.CreateObject(objectAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_EdPrivateKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Ed-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);


        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_EC_EDWARDS),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, Convert.FromHexString("06032B6570")),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Convert.FromHexString("670C6C66F47DA1EB61365137D6FB8299E6879F12BDED475694C6D0167FE903F5"))
        };

        IObjectHandle handle = session.CreateObject(privateKeyAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_EdPublicKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Ed-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);


        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_EC_EDWARDS),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, Convert.FromHexString("06032B6570")),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_POINT, Convert.FromHexString("17261FF1AE6D04FF6514F8DB90D3CDDBAC6F1B43536D8A68B3806C60BA497CD0"))
        };

        IObjectHandle handle = session.CreateObject(publicKeyAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_MontgomeryPrivateKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"X-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);


        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_EC_MONTGOMERY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, Convert.FromHexString("06032B656E")),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Convert.FromHexString("670C6C66F47DA1EB61365137D6FB8299E6879F12BDED475694C6D0167FE903F5"))
        };

        IObjectHandle handle = session.CreateObject(privateKeyAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_MongomeryPublicKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"X-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);


        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_EC_MONTGOMERY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, Convert.FromHexString("06032B656E")),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_POINT, Convert.FromHexString("17261FF1AE6D04FF6514F8DB90D3CDDBAC6F1B43536D8A68B3806C60BA497CD0"))
        };

        IObjectHandle handle = session.CreateObject(publicKeyAttributes);

        session.DestroyObject(handle);
    }
}
