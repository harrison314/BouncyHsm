using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using Pkcs11Interop.Ext;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T24_EncryptSalsa20
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(0UL, 64, 256)]
    [DataRow(0UL, 192, 256)]
    public void Encrypt_Salsa20_Success(ulong counter, int nonceBits, int plainTextLen)
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

        IObjectHandle key = this.GenerateSalsa20Key(session);

        byte[] nonce = new byte[nonceBits / 8];
        Random.Shared.NextBytes(nonce);
        using IMechanismParams salsaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20Params(counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20, salsaParams);

        byte[] cipherText = session.Encrypt(mechanism, key, plainText);

        Assert.IsNotNull(cipherText);
        Assert.HasCount(plainText.Length, cipherText, "Mismatch length.");
    }

    [TestMethod]
    [DataRow(0UL, 64, 256, 32)]
    [DataRow(0UL, 192, 256, 16)]
    public void Encrypt_Salsa20AsStream_Success(ulong counter, int nonceBits, int plainTextLen, int streamBufferLen)
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

        IObjectHandle key = this.GenerateSalsa20Key(session);

        byte[] nonce = new byte[nonceBits / 8];
        Random.Shared.NextBytes(nonce);
        using IMechanismParams salsaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20Params(counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20, salsaParams);

        using MemoryStream plainTextMs = new MemoryStream(plainText, false);
        plainTextMs.Position = 0L;
        using MemoryStream cipherTextMs = new MemoryStream();
        session.Encrypt(mechanism, key, plainTextMs, cipherTextMs, streamBufferLen);

        byte[] cipherText = cipherTextMs.ToArray();
        Assert.IsNotNull(cipherText);
        Assert.HasCount(plainText.Length, cipherText, "Mismatch length.");
    }

    //[TestMethod]
    //[DataRow(96, 256, 0)]
    //[DataRow(96, 256, 59)]
    //[DataRow(96, 217, 0)]
    //[DataRow(96, 217, 4)]
    //[DataRow(96, 43, 59)]
    //public void Encrypt_Salsa20Polly_Success(int nonceBits, int plainTextLen, int aadDataLen)
    //{
    //    byte[] plainText = new byte[plainTextLen];
    //    Random.Shared.NextBytes(plainText);

    //    byte[]? aadData = null;
    //    if (aadDataLen > 0)
    //    {
    //        aadData = new byte[aadDataLen];
    //        Random.Shared.NextBytes(aadData);
    //    }

    //    Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
    //    using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
    //        AssemblyTestConstants.P11LibPath,
    //        AppType.SingleThreaded);

    //    List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
    //    ISlot slot = slots.SelectTestSlot();

    //    using ISession session = slot.OpenSession(SessionType.ReadWrite);
    //    session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

    //    IObjectHandle key = this.GenerateChaCha20Key(session);

    //    byte[] nonce = new byte[nonceBits / 8];
    //    Random.Shared.NextBytes(nonce);
    //    using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20ChaCha20Polly1305Params(nonce, aadData);
    //    using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20_POLY1305, chachaParams);

    //    byte[] cipherText = session.Encrypt(mechanism, key, plainText);

    //    Assert.IsNotNull(cipherText);
    //}

    private IObjectHandle GenerateSalsa20Key(ISession session)
    {
        string label = $"Salsa20-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
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

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}