using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using Pkcs11Interop.Ext;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T24_EncryptChaCha20
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow(0, 64, 256)]
    //[DataRow(0, 96, 256)]
    //[DataRow(0, 192, 256)]
    //[DataRow(51, 64, 87)]
    //[DataRow(13, 96, 20786)]
    //[DataRow(13, 192, 478)]
    public void Encrypt_ChaCha20_Success(int counter, int nonceBits, int plainTextLen)
    {
        byte[] plainText = new byte[plainTextLen];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateChaCha20Key(session);

        byte[] nonce = new byte[nonceBits / 8];
        Random.Shared.NextBytes(nonce);
        using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkChaCha20Params((uint)counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20, chachaParams);

        byte[] cipherText = session.Encrypt(mechanism, key, plainText);

        Assert.IsNotNull(cipherText);
        Assert.AreEqual(plainText.Length, cipherText.Length, "Mismatch length.");
    }

    [DataTestMethod]
    [DataRow(0, 64, 256, 32)]
    //[DataRow(0, 96, 256, 16)]
    //[DataRow(0, 192, 256, 122)]
    //[DataRow(51, 64, 87, 52)]
    //[DataRow(13, 96, 20786, 200)]
    //[DataRow(13, 192, 478, 32)]
    public void Encrypt_ChaCha20AsStream_Success(int counter, int nonceBits, int plainTextLen, int streamBufferLen)
    {
        byte[] plainText = new byte[plainTextLen];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateChaCha20Key(session);

        byte[] nonce = new byte[nonceBits / 8];
        Random.Shared.NextBytes(nonce);
        using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkChaCha20Params((uint)counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20, chachaParams);

        using MemoryStream plainTextMs = new MemoryStream(plainText, false);
        plainTextMs.Position = 0L;
        using MemoryStream cipherTextMs = new MemoryStream();
        session.Encrypt(mechanism, key, plainTextMs, cipherTextMs, streamBufferLen);

        byte[] cipherText = cipherTextMs.ToArray();
        Assert.IsNotNull(cipherText);
        Assert.AreEqual(plainText.Length, cipherText.Length, "Mismatch length.");
    }

    [DataTestMethod]
    [DataRow(0, 64, 256)]
    //[DataRow(0, 96, 256)]
    //[DataRow(0, 192, 256)]
    public void Encrypt_ChaCha20With64BitCounter_Success(int counter, int nonceBits, int plainTextLen)
    {
        byte[] plainText = new byte[plainTextLen];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateChaCha20Key(session);

        byte[] nonce = new byte[nonceBits / 8];
        Random.Shared.NextBytes(nonce);
        using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkChaCha20Params((ulong)counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20, chachaParams);

        byte[] cipherText = session.Encrypt(mechanism, key, plainText);

        Assert.IsNotNull(cipherText);
        Assert.AreEqual(plainText.Length, cipherText.Length, "Mismatch length.");
    }

    private IObjectHandle GenerateChaCha20Key(ISession session)
    {
        string label = $"ChaCha20-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
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
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32U),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}