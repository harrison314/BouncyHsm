using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue164
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void DeriveKey_ConcatBaseAndKey_RejectsDisallowedMechanismOnSecondKey()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle baseKey = this.CreateSecret(session,
            factories,
            Enumerable.Repeat((byte)0x11, 16).ToArray(),
            CKM.CKM_CONCATENATE_BASE_AND_KEY);
        IObjectHandle secondKey = this.CreateSecret(session,
            factories,
            Enumerable.Repeat((byte)0x22, 16).ToArray(),
            CKM.CKM_CONCATENATE_BASE_AND_DATA);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_CONCATENATE_BASE_AND_KEY, BitConverter.GetBytes(secondKey.ObjectId));

        List<IObjectAttribute> derivedKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
        };

        Pkcs11Exception ex = Assert.ThrowsExactly<Pkcs11Exception>(() => session.DeriveKey(mechanism, baseKey, derivedKeyAttributes));

        Assert.AreEqual(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED, ex.RV);
    }

    private IObjectHandle CreateSecret(ISession session, Pkcs11InteropFactories factories, byte[] value, CKM allowedMechanism)
    {
        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, value),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ALLOWED_MECHANISMS, new List<CKM>() { allowedMechanism }),
        };

        return session.CreateObject(keyAttributes);
    }
}
