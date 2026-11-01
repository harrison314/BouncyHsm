using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignAes
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(16, 64)]
    [DataRow(24, 64)]
    [DataRow(32, 64)]
    [DataRow(32, 0)]
    [DataRow(32, 12)]
    [DataRow(32, 32)]
    [DataRow(32, 156)]
    public void Sign_AesCmac_Success(int size, int dataToSignSize)
    {
        byte[] dataToSign = new byte[dataToSignSize];
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
        Assert.IsNotNull(signature);

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(16, 8, 64)]
    [DataRow(24, 8, 64)]
    [DataRow(32, 8, 64)]
    [DataRow(16, 12, 64)]
    [DataRow(24, 4, 64)]
    [DataRow(32, 16, 64)]
    [DataRow(32, 8, 0)]
    [DataRow(32, 16, 32)]
    public void Sign_AesCmacGeneral_Success(int size, int parameter, int dataToSignSize)
    {
        byte[] dataToSign = new byte[dataToSignSize];
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
        Assert.IsNotNull(signature);

        session.DestroyObject(handle);
    }

    [TestMethod]
    [DataRow(16, 64)]
    [DataRow(24, 64)]
    [DataRow(32, 64)]
    [DataRow(32, 0)]
    [DataRow(32, 32)]
    [DataRow(32, 113)]
    public void Sign_AesGmac_Success(int size, int dataToSignSize)
    {
        byte[] dataToSign = new byte[dataToSignSize];
        byte[] iv = new byte[16];
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
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_AES_GMAC, iv);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);
        Assert.IsNotNull(signature);

        session.DestroyObject(handle);
    }

    private IObjectHandle GenerateAesKey(ISession session, int size)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

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
