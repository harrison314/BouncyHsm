using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Reflection.Metadata;
using System.Text;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T19_DestroyObject
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, "MyObject To delete"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_APPLICATION, "Tests"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Encoding.UTF8.GetBytes("Hello wold!")),
        };

        IObjectHandle handle = session.CreateObject(objectAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_SessionObject_Success()
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, "MyObject To delete"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_APPLICATION, "Tests"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Encoding.UTF8.GetBytes("Hello wold!")),
        };

        IObjectHandle handle = session.CreateObject(objectAttributes);

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void CreateObject_HwFeature_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = session.FindAllObjects(new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_HW_FEATURE)
        }).First();

        Pkcs11Exception exception = Assert.Throws<Pkcs11Exception>(() => session.DestroyObject(handle));
        Assert.AreEqual(CKR.CKR_OBJECT_HANDLE_INVALID, exception.RV);
    }

    [TestMethod]
    public void DestroyObject_TrustObject_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"TrustObject-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO_V3_2.CKO_TRUST),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),

            factories.ObjectAttributeFactory.Create(CKA.CKA_ISSUER, new Org.BouncyCastle.Asn1.X509.X509Name("CN=TestIssuer,C=SK").ToAsn1Object().GetEncoded()),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SERIAL_NUMBER, new Org.BouncyCastle.Asn1.DerInteger(12).ToAsn1Object().GetEncoded()),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HASH_OF_CERTIFICATE, new byte[32]),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MECHANISM_TYPE, (uint)CKM.CKM_SHA256),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_SERVER_AUTH, CKT_V3_2.CKT_NOT_TRUSTED),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_CLIENT_AUTH, CKT_V3_2.CKT_NOT_TRUSTED),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_CODE_SIGNING, CKT_V3_2.CKT_TRUSTED),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_EMAIL_PROTECTION, CKT_V3_2.CKT_NOT_TRUSTED),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_IPSEC_IKE, CKT_V3_2.CKT_NOT_TRUSTED),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_TIME_STAMPING, CKT_V3_2.CKT_TRUST_UNKNOWN),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_TRUST_OCSP_SIGNING, CKT_V3_2.CKT_TRUST_UNKNOWN),
        };

        IObjectHandle handle = session.CreateObject(objectAttributes);

        session.DestroyObject(handle);
    }
}