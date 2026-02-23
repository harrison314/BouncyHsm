using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_VerifyAes
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(16)]
    [DataRow(24)]
    [DataRow(32)]
    public void Verify_AesCmac_Success(int size)
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

        IObjectHandle handle = this.GenerateAesKey(session, size);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_AES_CMAC);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(16, 8)]
    [DataRow(24, 8)]
    [DataRow(32, 8)]
    [DataRow(16, 12)]
    [DataRow(24, 4)]
    [DataRow(32, 16)]
    public void Verify_AesCmacGeneral_Success(int size, int parameter)
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

        IObjectHandle handle = this.GenerateAesKey(session, size);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkMacGeneralParams mechanismParam = factories.MechanismParamsFactory.CreateCkMacGeneralParams((ulong)parameter);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_AES_CMAC_GENERAL, mechanismParam);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        session.Verify(mechanism, handle, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        signature[2] ^= 0x13;

        session.Verify(mechanism, handle, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");

        session.DestroyObject(handle);
    }

    private IObjectHandle GenerateAesKey(ISession session, int size)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}