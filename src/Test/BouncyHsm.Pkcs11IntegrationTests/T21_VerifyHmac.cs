using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Security.Cryptography;
using System.Drawing;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T21_VerifyHmac
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM.CKM_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM.CKM_SHA_1_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC, 64)]
    [DataRow(CKK.CKK_SHA512_HMAC, CKM.CKM_SHA512_HMAC, 64)]
    public void Verify_Hmac_Success(CKK type, CKM signatureMechanism, int size)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

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
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_224_HMAC, 28)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_256_HMAC, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_256_HMAC, 14)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_256_HMAC, 1)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_384_HMAC, 48)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_512_HMAC, 64)]
    public void Verify_HmacSha3_Success(CKK type, CKM signatureMechanism, int size)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

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
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_BLAKE2B_160_HMAC, 28)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_BLAKE2B_256_HMAC, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_BLAKE2B_384_HMAC, 14)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_BLAKE2B_256_HMAC, 1)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_BLAKE2B_384_HMAC, 48)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_BLAKE2B_512_HMAC, 64)]
    public void Verify_Blake2_Success(CKK type, CKM signatureMechanism, int size)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

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
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM.CKM_SHA256_HMAC_GENERAL, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM.CKM_SHA_1_HMAC_GENERAL, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC_GENERAL, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC_GENERAL, 64)]
    [DataRow(CKK.CKK_SHA512_HMAC, CKM.CKM_SHA512_HMAC_GENERAL, 64)]
    public void Verify_HmacGeneral_Success(CKK type, CKM signatureMechanism, int size)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

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
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkMacGeneralParams mechanismParam = factories.MechanismParamsFactory.CreateCkMacGeneralParams(4);
        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism, mechanismParam);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_224_HMAC_GENERAL, 28)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_256_HMAC_GENERAL, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_384_HMAC_GENERAL, 48)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_512_HMAC_GENERAL, 64)]
    public void Verify_HmacSha3General_Success(CKK type, CKM signatureMechanism, int size)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

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
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkMacGeneralParams mechanismParam = factories.MechanismParamsFactory.CreateCkMacGeneralParams(4);
        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism, mechanismParam);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_224_HMAC_GENERAL, 28)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_256_HMAC_GENERAL, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_384_HMAC_GENERAL, 48)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_SHA3_512_HMAC_GENERAL, 64)]
    public void Verify_HmacBlakeGeneral_Success(CKK type, CKM signatureMechanism, int size)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

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
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkMacGeneralParams mechanismParam = factories.MechanismParamsFactory.CreateCkMacGeneralParams(4);
        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism, mechanismParam);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    private void GenerateSeecret(CKK type, int size, Pkcs11InteropFactories factories, ISession session, string label, byte[] ckId)
    {
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_GENERIC_SECRET_KEY_GEN);
        _ = session.GenerateKey(mechanism, keyAttributes);
    }

    private IObjectHandle FindSeecretKey(ISession session, byte[] ckaId, string ckaLabel)
    {
        List<IObjectAttribute> searchTemplate = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, ckaLabel)
        };

        return session.FindAllObjects(searchTemplate).Single();
    }
}
