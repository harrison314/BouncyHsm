using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T22_GenerateKey
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, 5)]
    [DataRow(CKK.CKK_GENERIC_SECRET, 512)]
    [DataRow(CKK.CKK_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, 64)]
    [DataRow(CKK.CKK_SHA512_HMAC, 64)]
    public void Ganerate_GenericSeecret_Success(CKK type, int size)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, type),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_GENERIC_SECRET_KEY_GEN);

        IObjectHandle handle = session.GenerateKey(mechanism, keyAttributes);
        this.TestContext?.WriteLine("Object created");

        session.DestroyObject(handle);
    }

    [DataTestMethod]
    [DataRow(16)]
    [DataRow(24)]
    [DataRow(32)]
    public void Ganerate_AES_Success(int size)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        IObjectHandle handle = session.GenerateKey(mechanism, keyAttributes);
        this.TestContext?.WriteLine("Object created");

        session.DestroyObject(handle);
    }

    [DataTestMethod]
    [DataRow(CKM_V3_0.CKM_SHA_1_KEY_GEN, 20)]
    [DataRow(CKM_V3_0.CKM_SHA224_KEY_GEN, 25)]
    [DataRow(CKM_V3_0.CKM_SHA256_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_SHA384_KEY_GEN, 38)]
    [DataRow(CKM_V3_0.CKM_SHA512_KEY_GEN, 64)]
    [DataRow(CKM_V3_0.CKM_SHA512_224_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_SHA512_256_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_SHA512_224_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_SHA512_T_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_SHA3_224_KEY_GEN, 28)]
    [DataRow(CKM_V3_0.CKM_SHA3_256_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_SHA3_384_KEY_GEN, 38)]
    [DataRow(CKM_V3_0.CKM_SHA3_512_KEY_GEN, 64)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_160_KEY_GEN, 20)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_256_KEY_GEN, 32)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_256_KEY_GEN, 10)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_256_KEY_GEN, 512)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_384_KEY_GEN, 60)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_512_KEY_GEN, 64)]
    public void Ganerate_GenericHmacKey_Success(CKM mechanismType, int size)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            //factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, type),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType);

        IObjectHandle handle = session.GenerateKey(mechanism, keyAttributes);
        this.TestContext?.WriteLine("Object created");

        session.DestroyObject(handle);
    }

    [TestMethod]
    public void Ganerate_Ppoly1305_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Poly1305-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        IObjectHandle handle = session.GenerateKey(mechanism, keyAttributes);
        this.TestContext?.WriteLine("Object created");

        session.DestroyObject(handle);
    }
}
