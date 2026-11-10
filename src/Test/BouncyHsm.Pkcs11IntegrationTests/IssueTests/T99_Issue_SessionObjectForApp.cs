using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Reflection.Emit;
using System.Text;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue_SessionObjectForApp
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void SessionObject_FromAnotherSession_Find()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        string label = $"Data-{DateTime.UtcNow:s}-{Random.Shared.Next()}";

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        _ = this.CreateObject(session, label);

        using ISession anotherSession = slot.OpenSession(SessionType.ReadOnly);
        IObjectHandle? handle = this.TryFindObject(anotherSession, label);
        Assert.IsNotNull(handle, "Not found object in another session.");
    }

    [TestMethod]
    public void SessionObject_FromAnotherSession_GetAttributes()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        string label = $"Data-{DateTime.UtcNow:s}-{Random.Shared.Next()}";

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        _ = this.CreateObject(session, label);

        using ISession anotherSession = slot.OpenSession(SessionType.ReadOnly);
        IObjectHandle? handle = this.TryFindObject(anotherSession, label);
        Assert.IsNotNull(handle, "Not found object in another session.");

        List<IObjectAttribute> attributes = anotherSession.GetAttributeValue(handle, new List<CKA>() { CKA.CKA_PRIVATE, CKA.CKA_LABEL });

        Assert.AreEqual(label, attributes[1].GetValueAsString());
    }

    [TestMethod]
    public void SessionObject_FromAnotherSession_Destroy()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        string label = $"Data-{DateTime.UtcNow:s}-{Random.Shared.Next()}";
        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        _ = this.CreateObject(session, label);

        using ISession anotherSession = slot.OpenSession(SessionType.ReadOnly);
        IObjectHandle? handle = this.TryFindObject(anotherSession, label);
        Assert.IsNotNull(handle, "Not found object in another session.");
        session.DestroyObject(handle);

        Assert.IsNull(this.TryFindObject(session, label), "Object found in master session");
        Assert.IsNull(this.TryFindObject(anotherSession, label), "Object found in another session");
    }

    [TestMethod]
    public void SessionObject_FromAnotherSession_DestroyAnother()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        string label = $"Data-{DateTime.UtcNow:s}-{Random.Shared.Next()}";
        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        _ = this.CreateObject(session, label);

        using ISession anotherSession = slot.OpenSession(SessionType.ReadWrite);
        IObjectHandle? handle = this.TryFindObject(anotherSession, label);
        Assert.IsNotNull(handle, "Not found object in another session.");
        anotherSession.DestroyObject(handle);

        Assert.IsNull(this.TryFindObject(session, label), "Object found in master session");
        Assert.IsNull(this.TryFindObject(anotherSession, label), "Object found in another session");
    }

    [TestMethod]
    public void SessionObject_FromAnotherSession_DestroyAfterSessionClose()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession masterSession = slot.OpenSession(SessionType.ReadOnly);

        string label = $"Data-{DateTime.UtcNow:s}-{Random.Shared.Next()}";
        ISession? anotherSession = null;
        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            _ = this.CreateObject(session, label);

            anotherSession = slot.OpenSession(SessionType.ReadWrite);

            IObjectHandle? handle = this.TryFindObject(anotherSession, label);
            Assert.IsNotNull(handle, "Not found object in another session.");
        }

        Assert.IsNull(this.TryFindObject(masterSession, label), "Object found in master session");
        Assert.IsNull(this.TryFindObject(anotherSession, label), "Object found in another session");

        anotherSession?.Dispose();
    }

    private IObjectHandle CreateObject(ISession session, string label)
    {
        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Encoding.UTF8.GetBytes($"T99_Issue_SessionObjectForApp {DateTimeOffset.UtcNow}")),
        };

        return session.CreateObject(objectAttributes);
    }

    private IObjectHandle? TryFindObject(ISession session, string label)
    {
        List<IObjectHandle> objects = session.FindAllObjects(new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label)
        });

        return objects.SingleOrDefault();
    }
}