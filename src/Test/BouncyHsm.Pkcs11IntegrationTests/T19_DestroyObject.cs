using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
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
}