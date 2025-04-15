using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
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

    [DataTestMethod]
    [DataRow(CKM.CKM_MD2_KEY_DERIVATION)]
    [DataRow(CKM.CKM_MD5_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA1_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA224_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA256_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA384_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA512_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA512_224_KEY_DERIVATION)]
    [DataRow(CKM.CKM_SHA512_256_KEY_DERIVATION)]
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
        byte[] ckId = session.GenerateRandom(32);
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

    [DataTestMethod]
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
        byte[] ckId = session.GenerateRandom(32);
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
    //[DataTestMethod]
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
    //    byte[] ckId = session.GenerateRandom(32);
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

    [DataTestMethod]
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
        byte[] ckId = session.GenerateRandom(32);
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

    [DataTestMethod]
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
        byte[] ckId = session.GenerateRandom(32);
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

    [DataTestMethod]
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
        byte[] ckId = session.GenerateRandom(32);
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

    private IObjectHandle CreateSecret(ISession session, byte[] data)
    {
        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);
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
