using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue165
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Sign_WithAesKey_ThrowsKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.CreateAesKey(session);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_ECDSA);
        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.Sign(mechanism, key, new byte[32]));

        Assert.AreEqual(CKR.CKR_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    [TestMethod]
    public void Verify_WithAesKey_ThrowsKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.CreateAesKey(session);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_ECDSA);
        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.Verify(mechanism, key, new byte[32], new byte[64], out _));

        Assert.AreEqual(CKR.CKR_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    [TestMethod]
    public void Encrypt_WithEcKey_ThrowsKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        this.CreateEcKeyPair(session, out IObjectHandle publicKey, out _);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC, new byte[16]);
        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.Encrypt(mechanism, publicKey, new byte[16]));

        Assert.AreEqual(CKR.CKR_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    [TestMethod]
    public void Decrypt_WithEcKey_ThrowsKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        this.CreateEcKeyPair(session, out _, out IObjectHandle privateKey);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC, new byte[16]);
        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.Decrypt(mechanism, privateKey, new byte[16]));

        Assert.AreEqual(CKR.CKR_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    [TestMethod]
    public void Derive_EcdhWithAesKey_ThrowsKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.CreateAesKey(session);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams parameters =
            session.Factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)CKD.CKD_NULL, null, P256Generator);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, parameters);
        List<IObjectAttribute> template = this.CreateDerivedKeyTemplate(session);

        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.DeriveKey(mechanism, key, template));

        Assert.AreEqual(CKR.CKR_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    [TestMethod]
    public void Derive_HkdfWithEcKey_ThrowsKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        this.CreateEcKeyPair(session, out _, out IObjectHandle privateKey);

        using ICkHkdfParams parameters = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkHkdfParams(true,
            true,
            CKM.CKM_SHA256,
            0x00000001,
            null,
            Array.Empty<byte>(),
            Array.Empty<byte>());

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_HKDF_DERIVE, parameters);
        List<IObjectAttribute> template = this.CreateDerivedKeyTemplate(session);

        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.DeriveKey(mechanism, privateKey, template));

        Assert.AreEqual(CKR.CKR_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    [TestMethod]
    public void Wrap_WithEcKey_ThrowsWrappingKeyTypeInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        this.CreateEcKeyPair(session, out IObjectHandle publicKey, out _);
        IObjectHandle keyToWrap = this.CreateAesKey(session);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC, new byte[16]);
        Pkcs11Exception exception = Assert.ThrowsExactly<Pkcs11Exception>(() => session.WrapKey(mechanism, publicKey, keyToWrap));

        Assert.AreEqual(CKR.CKR_WRAPPING_KEY_TYPE_INCONSISTENT, exception.RV);
    }

    private IObjectHandle CreateAesKey(ISession session)
    {
        Pkcs11InteropFactories factories = session.Factories;
        List<IObjectAttribute> attributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_AES),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, new byte[32]),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true)
        };

        return session.CreateObject(attributes);
    }

    private void CreateEcKeyPair(ISession session, out IObjectHandle publicKey, out IObjectHandle privateKey)
    {
        Pkcs11InteropFactories factories = session.Factories;
        List<IObjectAttribute> publicAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, new byte[] { 0x06, 0x08, 0x2a, 0x86, 0x48, 0xce, 0x3d, 0x03, 0x01, 0x07 }),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true)
        };

        List<IObjectAttribute> privateAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_EC_KEY_PAIR_GEN);
        session.GenerateKeyPair(mechanism, publicAttributes, privateAttributes, out publicKey, out privateKey);
    }

    private List<IObjectAttribute> CreateDerivedKeyTemplate(ISession session)
    {
        Pkcs11InteropFactories factories = session.Factories;
        return new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32)
        };
    }

    private static readonly byte[] P256Generator =
    {
        0x04, 0x6b, 0x17, 0xd1, 0xf2, 0xe1, 0x2c, 0x42, 0x47, 0xf8, 0xbc, 0xe6, 0xe5, 0x63, 0xa4, 0x40, 0xf2,
        0x77, 0x03, 0x7d, 0x81, 0x2d, 0xeb, 0x33, 0xa0, 0xf4, 0xa1, 0x39, 0x45, 0xd8, 0x98, 0xc2, 0x96,
        0x4f, 0xe3, 0x42, 0xe2, 0xfe, 0x1a, 0x7f, 0x9b, 0x8e, 0xe7, 0xeb, 0x4a, 0x7c, 0x0f, 0x9e, 0x16,
        0x2b, 0xce, 0x33, 0x57, 0x6b, 0x31, 0x5e, 0xce, 0xcb, 0xb6, 0x40, 0x68, 0x37, 0xbf, 0x51, 0xf5
    };
}
