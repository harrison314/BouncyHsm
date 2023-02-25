using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Text;
using PkcsExtensions;
using System.Security.Cryptography;

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


        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>
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

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100,999)}";
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
}
