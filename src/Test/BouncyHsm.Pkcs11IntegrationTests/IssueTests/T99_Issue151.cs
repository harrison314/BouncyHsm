using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using PkcsExtensions;
using System.Data;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue151
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void DeriveKey_XorBaseAndData_ThrowsTemplateIncomplete()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Utils.GetRandomBytes(32, true));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId)
        };

        byte[] zeros = new byte[16];
        Array.Fill(zeros, (byte)0x00);
        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam = factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(zeros);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_XOR_BASE_AND_DATA, mechanismParam);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => _ = session.DeriveKey(mechanism, handle, newKeyAttributes));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCOMPLETE, ex.RV);
    }

    private IObjectHandle CreateSecret(ISession session, byte[] data)
    {
        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);
        Pkcs11InteropFactories factories = session.Factories;

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, data),
        };

        return session.CreateObject(keyAttributes);
    }
}
