using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using PkcsExtensions;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T23_DeriveKey
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKM.CKM_MD2_KEY_DERIVATION)]
    [DataRow(CKM.CKM_MD5_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA1_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA224_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA256_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA384_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA512_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA512_224_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA512_256_KEY_DERIVATION)]
    [DataRow(CKM_V3_0.CKM_SHAKE_128_KEY_DERIVATION)]
    [DataRow(CKM_V3_0.CKM_SHAKE_256_KEY_DERIVATION)]
    public void Derive_Digest_Success(CKM mechanismType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 });

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    [DataRow(CKM.CKM_CONCATENATE_BASE_AND_DATA, "0102030405060708", "AABBCCDDEE", "0102030405060708AABBCCDDEE")]
    [DataRow(CKM.CKM_CONCATENATE_DATA_AND_BASE, "0102030405060708", "AABBCCDDEE", "AABBCCDDEE0102030405060708")]
    [DataRow(CKM.CKM_XOR_BASE_AND_DATA, "0102030405060708", "AABBCCDDEE", "ABB9CFD9EBACBCC4")]
    public void Derive_ConcatData_Success(CKM mechanismType, string baseHex, string dataHex, string resultHex)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, HexConvertor.GetBytes(baseHex));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam = factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(HexConvertor.GetBytes(dataHex));
        using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType, mechanismParam);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        byte[] result = this.GetValue(session, derivedHandle);
        byte[] exceptedResult = HexConvertor.GetBytes(resultHex);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);

        Assert.AreEqual(Convert.ToHexString(exceptedResult), Convert.ToHexString(result));
    }

    // Toto preverit s jarom https://www.cryptsoft.com/pkcs11doc/v210/group__SEC__12__37__1__CONCATENATION__OF__A__BASE__KEY__AND__ANOTHER__KEY.html
    //[TestMethod]
    //[DataRow(CKM.CKM_CONCATENATE_BASE_AND_KEY, "0102030405060708", "AABBCCDDEE", "0102030405060708AABBCCDDEE")]
    //public void Derive_ConcatKeys_Success(CKM mechanismType, string baseHex, string dataHex, string resultHex)
    //{
    //    Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
    //    using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
    //        AssemblyTestConstants.P11LibPath,
    //        AppType.SingleThreaded);

    //    List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
    //    ISlot slot = slots.SelectTestSlot();

    //    using ISession session = slot.OpenSession(SessionType.ReadWrite);
    //    session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

    //    IObjectHandle handle = this.CreateSecret(session, HexConvertor.GetBytes(baseHex));
    //    IObjectHandle handle2 = this.CreateSecret(session, HexConvertor.GetBytes(dataHex));

    //    string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
    //    byte[] ckId = Utils.GetRandomArray(32);
    //    List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
    //    {
    //        //factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
    //        //factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

    //        factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
    //        factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
    //    };

    //    //Missing paramas
    //    using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam = factories.MechanismParamsFactory.cr
    //    using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType, handle2);
    //    IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);


    //    byte[] result = this.GetValue(session, derivedHandle);
    //    byte[] exceptedResult = HexConvertor.GetBytes(resultHex);

    //    session.DestroyObject(handle);
    //    session.DestroyObject(derivedHandle);

    //    Assert.AreEqual(Convert.ToHexString(exceptedResult), Convert.ToHexString(result));
    //}

    [TestMethod]
    [DataRow("1202030405060708", 8U, "12")]
    [DataRow("1202030405060708", 32U, "12020304")]
    [DataRow("120203A405060708", 35U, "120203A400")]
    [DataRow("120203FFFFFFFFFF", 39U, "120203FFFE")]
    public void Derive_ExtractKey_Success(string baseHex, uint extract, string resultHex)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, HexConvertor.GetBytes(baseHex));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkExtractParams mechanismParam = factories.MechanismParamsFactory.CreateCkExtractParams(extract);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_EXTRACT_KEY_FROM_KEY, mechanismParam);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        byte[] result = this.GetValue(session, derivedHandle);
        byte[] exceptedResult = HexConvertor.GetBytes(resultHex);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);

        Assert.AreEqual(Convert.ToHexString(exceptedResult), Convert.ToHexString(result));
    }

    [TestMethod]
    [DataRow(CKM_V3_0.CKM_SHA3_224_KEY_DERIVATION)]
    [DataRow(CKM_V3_0.CKM_SHA3_256_KEY_DERIVATION)]
    [DataRow(CKM_V3_0.CKM_SHA3_384_KEY_DERIVATION)]
    [DataRow(CKM_V3_0.CKM_SHA3_512_KEY_DERIVATION)]
    public void Derive_Sha3Digest_Success(CKM mechanismType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 });

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_160_KEY_DERIVE)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_256_KEY_DERIVE)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_384_KEY_DERIVE)]
    [DataRow(CKM_V3_0.CKM_BLAKE2B_512_KEY_DERIVE)]
    public void Derive_Blake2bDigest_Success(CKM mechanismType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 });

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(mechanismType);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    [DataRow(true, true, CKM.CKM_SHA256, 0x00000001u, 32)]
    [DataRow(false, true, CKM.CKM_SHA256, 0x00000001u, 32)]
    [DataRow(true, false, CKM.CKM_SHA256, 0x00000001u, 32)]
    [DataRow(true, false, CKM.CKM_SHA512, 0x00000001u, 64)]
    [DataRow(true, true, CKM.CKM_SHA256, 0x00000002u, 32)]
    [DataRow(true, true, CKM.CKM_SHA256, 0x00000004u, 32)]
    [DataRow(true, true, CKM_V3_0.CKM_SHA3_256, 0x00000002u, 32)]
    [DataRow(true, true, CKM_V3_0.CKM_SHA3_512, 0x00000002u, 32)]
    public void Derive_Hkdf_Success(bool extract, bool expand, CKM digestMechanism, uint satlType, int saltSize)
    {
        byte[] salt = new byte[saltSize];
        byte[] info = new byte[32];

        Random.Shared.NextBytes(salt);
        Random.Shared.NextBytes(info);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 });
        IObjectHandle saltKey = this.CreateSecret(session, new byte[] { 1, 4, 5, 9, 7, 5, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 47, 12, 5, 241, 111, 123, 0, 0, 0, 7 });

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using ICkHkdfParams hkdfParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkHkdfParams(extract,
            expand,
            digestMechanism,
            satlType,
            saltKey,
            salt,
            info);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_HKDF_DERIVE, hkdfParams);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    public void Derive_WithPermitedAlgorithm_Failed()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Utils.GetRandomBytes(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ALLOWED_MECHANISMS, new List<CKM>(){CKM_V3_0.CKM_SHAKE_128_KEY_DERIVATION, CKM_V3_0.CKM_SHAKE_256_KEY_DERIVATION }),
        };

        IObjectHandle handle = session.CreateObject(keyAttributes);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256_KEY_DERIVATION);
        Pkcs11Exception ex = Assert.ThrowsExactly<Pkcs11Exception>(() => session.DeriveKey(mechanism, handle, newKeyAttributes));
        Assert.AreEqual(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED, ex.RV);

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(false, false)]
    public void Derive_WithCorrectDeriveTemplate_Success(bool verify, bool modifiable)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);
        List<IObjectAttribute> deriveTemplateAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, verify),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, modifiable),
        };

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE_TEMPLATE, deriveTemplateAttributes),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 }),
        };

        IObjectHandle handle = session.CreateObject(keyAttributes);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256_KEY_DERIVATION);

        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);

        List<IObjectAttribute> storedAttributes = session.GetAttributeValue(derivedHandle, new List<CKA>()
        {
            CKA.CKA_VERIFY,
            CKA.CKA_MODIFIABLE
        });

        Assert.AreEqual(verify, storedAttributes[0].GetValueAsBool(), "Mismatch CKA_VERIFY");
        Assert.AreEqual(modifiable, storedAttributes[1].GetValueAsBool(), "Mismatch CKA_MODIFIABLE");

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    public void Derive_WithCorrectDeriveTemplate_Throws()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);
        List<IObjectAttribute> deriveTemplateAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, false),
        };

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE_TEMPLATE, deriveTemplateAttributes),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 }),
        };

        IObjectHandle handle = session.CreateObject(keyAttributes);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256_KEY_DERIVATION);
        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.DeriveKey(mechanism, handle, newKeyAttributes));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV);
    }


    [TestMethod]
    [DataRow(CKM.CKM_SHA_1_HMAC)]
    [DataRow(CKM.CKM_SHA224_HMAC)]
    [DataRow(CKM.CKM_SHA256_HMAC)]
    [DataRow(CKM.CKM_SHA384_HMAC)]
    [DataRow(CKM.CKM_SHA512_HMAC)]
    [DataRow(CKM_V3_0.CKM_SHA3_224_HMAC)]
    [DataRow(CKM_V3_0.CKM_SHA3_256_HMAC)]
    [DataRow(CKM_V3_0.CKM_SHA3_384_HMAC)]
    [DataRow(CKM_V3_0.CKM_SHA3_512_HMAC)]
    [DataRow(CKM.CKM_AES_CMAC)]
    public void Derive_Sp800108CounterKdf_Success(CKM prfType)
    {
        string labelValue = "99c3d79cb978724e1e2f09dc90e3b694";
        string contextValue = "18582cd847d60455fb88924c9fd8fb63";

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Utils.GetRandomBytes(32));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32U),
        };

        using ICkSP800_108KdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateSp800_108KdfParams(prfType,
             null,
             new List<KdfDataParam>()
             {
                new KdfDataParam.IterationVariable(false, 32),
                new KdfDataParam.ByteArray(Convert.FromHexString(labelValue)),
                new KdfDataParam.ByteArray(new byte[] { 0x00 }),
                new KdfDataParam.ByteArray(Convert.FromHexString(contextValue)),
                new KdfDataParam.DkmLength(false, 32, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)
             });

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_COUNTER_KDF, derivationMechanismParams);

        IObjectHandle derivedHandle = session.DeriveKey(derivationMechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    public void Derive_Sp800108CounterKdf_Data()
    {
        string key = "26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d";
        string labelValue = "99c3d79cb978724e1e2f09dc90e3b694";
        string contextValue = "18582cd847d60455fb88924c9fd8fb63";
        string result = "16C6704DF3F2C2E49169DBE902";

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Convert.FromHexString(key));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 13U),
        };

        using ICkSP800_108KdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateSp800_108KdfParams(CKM.CKM_SHA256_HMAC,
             null,
             new List<KdfDataParam>()
             {
                new KdfDataParam.IterationVariable(false, 32),
                new KdfDataParam.ByteArray(Convert.FromHexString(labelValue)),
                new KdfDataParam.ByteArray(new byte[] { 0x00 }),
                new KdfDataParam.ByteArray(Convert.FromHexString(contextValue)),
                new KdfDataParam.DkmLength(false, 32, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)
             });

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_COUNTER_KDF, derivationMechanismParams);

        IObjectHandle derivedHandle = session.DeriveKey(derivationMechanism, handle, newKeyAttributes);
        byte[] derivedSecret = session.GetAttributeValue(derivedHandle, new List<CKA>() { CKA.CKA_VALUE }).Single().GetValueAsByteArray();

        Assert.AreEqual(result, Convert.ToHexString(derivedSecret), true);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    [DataRow(8, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(16, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(24, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(32, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(40, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(48, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(56, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(8, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(16, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(24, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(32, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(40, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(48, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(56, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(64, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS)]
    [DataRow(64, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS)]
    public void Derive_Sp800108CounterKdfMoreData_Success(int variableSize, bool littleEndian, uint lengthMethod)
    {
        string key = "26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d";
        string labelValue = "99c3d79cb978724e1e2f09dc90e3b694";
        string contextValue = "18582cd847d60455fb88924c9fd8fb63";

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Convert.FromHexString(key));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 13U),
        };

        using ICkSP800_108KdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateSp800_108KdfParams(CKM.CKM_SHA256_HMAC,
             null,
             new List<KdfDataParam>()
             {
                 new KdfDataParam.IterationVariable(littleEndian, variableSize),
                 new KdfDataParam.ByteArray(Convert.FromHexString(labelValue)),
                 new KdfDataParam.ByteArray(new byte[] { 0x00 }),
                 new KdfDataParam.ByteArray(Convert.FromHexString(contextValue)),
                 new KdfDataParam.DkmLength(littleEndian, variableSize, lengthMethod)
             });

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_COUNTER_KDF, derivationMechanismParams);

        IObjectHandle derivedHandle = session.DeriveKey(derivationMechanism, handle, newKeyAttributes);
        byte[] derivedSecret = session.GetAttributeValue(derivedHandle, new List<CKA>() { CKA.CKA_VALUE }).Single().GetValueAsByteArray();


        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    public void Derive_Sp800108CounterKdfWithKeyData_Data()
    {
        string key = "26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d";
        string contextValue = "18582cd847d60455fb88924c9fd8fb63";

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Convert.FromHexString(key));
        IObjectHandle otherKey = this.CreateSecret(session, Utils.GetRandomBytes(32));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 13U),
        };

        using ICkSP800_108KdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateSp800_108KdfParams(CKM.CKM_SHA256_HMAC,
             null,
             new List<KdfDataParam>()
             {
                 new KdfDataParam.IterationVariable(false, 32),
                 new KdfDataParam.ByteArray(Convert.FromHexString(contextValue)),
                 new KdfDataParam.KeyHandle(otherKey),
                 new KdfDataParam.DkmLength(false, 32, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)
             });

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_COUNTER_KDF, derivationMechanismParams);

        IObjectHandle derivedHandle = session.DeriveKey(derivationMechanism, handle, newKeyAttributes);
        byte[] derivedSecret = session.GetAttributeValue(derivedHandle, new List<CKA>() { CKA.CKA_VALUE }).Single().GetValueAsByteArray();

        session.DestroyObject(otherKey);
        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    [DataRow(CKM.CKM_SHA_1_HMAC, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA224_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA384_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA512_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_224_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_256_HMAC, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_384_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_512_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_AES_CMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 8, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 24, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 32, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 40, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 48, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 56, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 8, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 24, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 32, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 40, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 48, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 56, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 64, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 64, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS)]
    public void Derive_Sp800108DoublePipelineKdfMoreData_Success(CKM kdf, int variableSize, bool littleEndian, uint lengthMethod)
    {
        string key = "26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d";
        string labelValue = "99c3d79cb978724e1e2f09dc90e3b694";

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Convert.FromHexString(key));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 13U),
        };

        using ICkSP800_108KdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateSp800_108KdfParams(kdf,
             null,
             new List<KdfDataParam>()
             {
                 new KdfDataParam.IterationVariable(littleEndian, variableSize),
                 new KdfDataParam.ByteArray(Convert.FromHexString(labelValue)),
                 new KdfDataParam.Counter(littleEndian, Math.Min(64, variableSize)),
                 new KdfDataParam.DkmLength(littleEndian, Math.Min(64, variableSize), lengthMethod)
             });

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_DOUBLE_PIPELINE_KDF, derivationMechanismParams);

        IObjectHandle derivedHandle = session.DeriveKey(derivationMechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    [TestMethod]
    [DataRow(CKM.CKM_SHA_1_HMAC, 16, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA224_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA384_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA512_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_224_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_256_HMAC, 16, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_384_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM_V3_0.CKM_SHA3_512_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_AES_CMAC, 64, 16, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 8, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 16, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 24, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 32, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 40, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 48, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 56, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 8, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 16, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 24, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 32, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 40, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 48, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 56, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 64, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 64, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 16, 64, false, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 0, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 4, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    [DataRow(CKM.CKM_SHA256_HMAC, 32, 128, true, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS)]
    public void Derive_Sp800108FfeedbackKdfMoreData_Success(CKM kdf, int ivLen, int variableSize, bool littleEndian, uint lengthMethod)
    {
        string key = "26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d";
        string labelValue = "99c3d79cb978724e1e2f09dc90e3b694";
        byte[] iv = new byte[ivLen];
        Random.Shared.NextBytes(iv);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.CreateSecret(session, Convert.FromHexString(key));

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 13U),
        };

        using ICkSP800_108FeedbackKdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSP800_108FeedbackKdfParams(kdf,
             null,
             iv,
             new List<KdfDataParam>()
             {
                 new KdfDataParam.IterationVariable(littleEndian, variableSize),
                 new KdfDataParam.ByteArray(Convert.FromHexString(labelValue)),
                 new KdfDataParam.Counter(littleEndian, Math.Min(64, variableSize)),
                 new KdfDataParam.DkmLength(littleEndian, Math.Min(64, variableSize), lengthMethod)
             });

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_FEEDBACK_KDF, derivationMechanismParams);

        IObjectHandle derivedHandle = session.DeriveKey(derivationMechanism, handle, newKeyAttributes);

        session.DestroyObject(handle);
        session.DestroyObject(derivedHandle);
    }

    private IObjectHandle CreateSecret(ISession session, byte[] data)
    {
        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32);
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

    private byte[] GetValue(ISession session, IObjectHandle handle)
    {
        return session.GetAttributeValue(handle, new List<CKA>() { CKA.CKA_VALUE })[0].GetValueAsByteArray();
    }
}
