using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T25_DecryptCamellia
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Decrypt_CamelliaEcb_Success()
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

        IObjectHandle key = this.GenerateCamelliaKey(session, 32);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_CAMELLIA_ECB);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    [TestMethod]
    public void Decrypt_CamelliaEcbWithParts_Success()
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

        IObjectHandle key = this.GenerateCamelliaKey(session, 32);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_CAMELLIA_ECB);
        using MemoryStream plainTextMs = new MemoryStream(plainText);
        using MemoryStream ciperTextMs = new MemoryStream();
        session.Encrypt(mechanism, key, plainTextMs, ciperTextMs, 32);

        ciperTextMs.Position = 0L;
        using MemoryStream decrypted = new MemoryStream();
        session.Decrypt(mechanism, key, ciperTextMs, decrypted, 32);
    }

    [TestMethod]
    [DataRow(CKM.CKM_CAMELLIA_CBC)]
    [DataRow(CKM.CKM_CAMELLIA_CBC_PAD)]
    public void Decrypt_CamelliaWithIv_Success(CKM mechanismType)
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

        IObjectHandle key = this.GenerateCamelliaKey(session, 32);
        byte[] iv = session.GenerateRandom(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(mechanismType, iv);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    public IObjectHandle GenerateCamelliaKey(ISession session, int size)
    {
        string label = $"Camellia-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_CAMELLIA_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}