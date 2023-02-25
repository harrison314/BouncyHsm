using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T15_Digest
{
    [DataTestMethod]
    [DataRow(CKM.CKM_MD2)]
    [DataRow(CKM.CKM_MD5)]
    [DataRow(CKM.CKM_SHA_1)]
    [DataRow(CKM.CKM_SHA224)]
    [DataRow(CKM.CKM_SHA256)]
    [DataRow(CKM.CKM_SHA384)]
    [DataRow(CKM.CKM_SHA512)]
    [DataRow(CKM.CKM_SHA512_256)]
    [DataRow(CKM.CKM_SHA512_224)]
    [DataRow(CKM.CKM_RIPEMD128)]
    [DataRow(CKM.CKM_RIPEMD160)]
    [DataRow(CKM.CKM_GOSTR3411)]
    public void Digest_Call_Success(CKM digestMechanism)
    {
        byte[] message = PkcsExtensions.HexConvertor.GetBytes("ba8b0702598482a4ef6d6f4056ea10a2e08ecfa327d5ebe9cb7cee0023cc9552362824e13cbb04b0f630bb8e1a3fa532c12fabbce5b4ca0283f2bfb79d67");

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        using IMechanism mechanism = factories.MechanismFactory.Create(digestMechanism);
        byte[] digest = session.Digest(mechanism, message);

        Assert.IsNotNull(digest);
    }

    [TestMethod]
    public void Digest_WithLongData_Success()
    {
        byte[] message = new byte[15_000];
        Random.Shared.NextBytes(message);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256);
        byte[] digest = session.Digest(mechanism, message);

        Assert.IsNotNull(digest);
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_MD2)]
    [DataRow(CKM.CKM_MD5)]
    [DataRow(CKM.CKM_SHA_1)]
    [DataRow(CKM.CKM_SHA224)]
    [DataRow(CKM.CKM_SHA256)]
    [DataRow(CKM.CKM_SHA384)]
    [DataRow(CKM.CKM_SHA512)]
    [DataRow(CKM.CKM_SHA512_256)]
    [DataRow(CKM.CKM_SHA512_224)]
    [DataRow(CKM.CKM_RIPEMD128)]
    [DataRow(CKM.CKM_RIPEMD160)]
    [DataRow(CKM.CKM_GOSTR3411)]
    public void DigestUpdate_Call_Success(CKM digestMechanism)
    {
        byte[] message = PkcsExtensions.HexConvertor.GetBytes("ba8b0702598482a4ef6d6f4056ea10a2e08ecfa327d5ebe9cb7cee0023cc9552362824e13cbb04b0f630bb8e1a3fa532c12fabbce5b4ca0283f2bfb79d67");

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        using MemoryStream ms = new MemoryStream(message, false);
        using IMechanism mechanism = factories.MechanismFactory.Create(digestMechanism);
        byte[] digest = session.Digest(mechanism, ms, 32);

        Assert.IsNotNull(digest);
    }

    [TestMethod]
    public void Digest_SHA512T_CallWithParameter_Success()
    {
        byte[] message = PkcsExtensions.HexConvertor.GetBytes("ba8b0702598482a4ef6d6f4056ea10a2e08ecfa327d5ebe9cb7cee0023cc9552362824e13cbb04b0f630bb8e1a3fa532c12fabbce5b4ca0283f2bfb79d67");

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        using ICkMacGeneralParams mechanismParameter = factories.MechanismParamsFactory.CreateCkMacGeneralParams(256);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA512_T, mechanismParameter);
        byte[] digest = session.Digest(mechanism, message);

        Assert.IsNotNull(digest);
    }

    [TestMethod]
    public void Digest_DigetKey_Success()
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

        byte[] secret = new byte[156];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        IObjectHandle keyHandle = session.CreateObject(objectAttributes);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256);
        byte[] digest = session.DigestKey(mechanism, keyHandle);

        Assert.IsNotNull(digest);
    }
}