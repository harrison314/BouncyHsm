using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T24_Encrypt
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Encrypt_AesEcb_Success()
    {
        byte[] plainText = new byte[16];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_ECB);
        byte[] chiperText = session.Encrypt(mechanism, key, plainText);

        Assert.IsNotNull(chiperText);
    }

    [TestMethod]
    public void Encrypt_AesEcbWithParts_Success()
    {
        byte[] plainText = new byte[160];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_ECB);
        using MemoryStream plainTextMs = new MemoryStream(plainText);
        using MemoryStream ciperTextMs = new MemoryStream();
        session.Encrypt(mechanism, key, plainTextMs, ciperTextMs, 20);
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_AES_CBC)]
    [DataRow(CKM.CKM_AES_CBC_PAD)]
    [DataRow(CKM.CKM_AES_CFB1)]
    [DataRow(CKM.CKM_AES_CFB8)]
    [DataRow(CKM.CKM_AES_CFB64)]
    [DataRow(CKM.CKM_AES_CFB128)]
    [DataRow(CKM.CKM_AES_OFB)]
    [DataRow(CKM.CKM_AES_CTR)]
    [DataRow(CKM.CKM_AES_CTS)]
    public void Encrypt_AesWithIv_Success(CKM mechanismType)
    {
        byte[] plainText = new byte[16];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = session.GenerateRandom(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(mechanismType, iv);
        byte[] chiperText = session.Encrypt(mechanism, key, plainText);

        Assert.IsNotNull(chiperText);
    }

    public IObjectHandle GenerateAesKey(ISession session, int size)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}