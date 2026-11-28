using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue161
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKM.CKM_CONCATENATE_BASE_AND_DATA)]
    [DataRow(CKM.CKM_CONCATENATE_DATA_AND_BASE)]
    public void DeriveKey_ProtectedBaseWithShortValueLen_ThrowsTemplateInconsistent(CKM mechanismType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle baseKey = this.CreateSecret(session, Utils.GetRandomBytes(16), sensitive: true, extractable: false);
        List<IObjectAttribute> derivedAttributes = this.CreateDerivedAttributes(session.Factories, valueLen: 1, canEncrypt: true);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam =
            factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(Utils.GetRandomBytes(16));
        using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType, mechanismParam);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => _ = session.DeriveKey(mechanism, baseKey, derivedAttributes));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV);
    }

    [TestMethod]
    public void DeriveKey_ExtractProtectedBaseWithShortValueLen_ThrowsTemplateInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle baseKey = this.CreateSecret(session, new byte[16], sensitive: true, extractable: false);
        List<IObjectAttribute> derivedAttributes = this.CreateDerivedAttributes(session.Factories, valueLen: 1);

        for (uint offset = 0; offset < 16 * 8; offset += 8)
        {
            using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkExtractParams mechanismParam =
                factories.MechanismParamsFactory.CreateCkExtractParams(offset);
            using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_EXTRACT_KEY_FROM_KEY, mechanismParam);

            Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() =>
                _ = session.DeriveKey(mechanism, baseKey, derivedAttributes));
            Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV, $"Offset {offset}.");
        }
    }

    [TestMethod]
    public void DeriveKey_XorProtectedBaseWithShortValueLen_ThrowsTemplateInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle baseKey = this.CreateSecret(session, new byte[16], sensitive: true, extractable: false);
        List<IObjectAttribute> derivedAttributes = this.CreateDerivedAttributes(session.Factories, valueLen: 1, canSign: true);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam =
            factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(new byte[16]);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_XOR_BASE_AND_DATA, mechanismParam);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => _ = session.DeriveKey(mechanism, baseKey, derivedAttributes));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV);
    }

    [TestMethod]
    public void DeriveKey_ConcatDataAndBase_KnownPrefixAndOneUnknownByte_ThrowsTemplateInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle baseKey = this.CreateSecret(session, new byte[16], sensitive: true, extractable: false);
        List<IObjectAttribute> derivedAttributes = this.CreateDerivedAttributes(session.Factories, valueLen: 16,
            keyType: CKK.CKK_AES, canEncrypt: true);
        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam =
            factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(Enumerable.Repeat((byte)0xA5, 15).ToArray());
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_CONCATENATE_DATA_AND_BASE, mechanismParam);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() =>
            _ = session.DeriveKey(mechanism, baseKey, derivedAttributes));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV);
    }

    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, false)]
    public void DigestKey_ProtectedKey_ThrowsKeyIndigestible(bool sensitive, bool extractable)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.CreateSecret(session, new byte[16], sensitive, extractable);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => _ = session.DigestKey(mechanism, key));
        Assert.AreEqual(CKR.CKR_KEY_INDIGESTIBLE, ex.RV);
    }

    private IObjectHandle CreateSecret(ISession session, byte[] value, bool sensitive, bool extractable)
    {
        Pkcs11InteropFactories factories = session.Factories;
        string label = $"Issue161-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";

        List<IObjectAttribute> attributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, sensitive),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, extractable),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, value)
        };

        return session.CreateObject(attributes);
    }

    private List<IObjectAttribute> CreateDerivedAttributes(Pkcs11InteropFactories factories, uint valueLen,
        CKK keyType = CKK.CKK_GENERIC_SECRET, bool canSign = false, bool canEncrypt = false)
    {
        return new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, keyType),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, valueLen),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, canSign),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, canEncrypt)
        };
    }
}
