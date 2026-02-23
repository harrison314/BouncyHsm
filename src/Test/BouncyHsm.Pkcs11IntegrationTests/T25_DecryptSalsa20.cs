using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using Pkcs11Interop.Ext;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T25_DecryptSalsa20
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(0UL, 64, 256)]
    [DataRow(0UL, 192, 256)]
    //[DataRow(0, 192, 256)]
    //[DataRow(51, 64, 87)]
    //[DataRow(13, 96, 20786)]
    //[DataRow(13, 192, 478)]
    public void Decrypt_Salsa20_Success(ulong counter, int nonceBits, int plainTextLen)
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

        byte[] cipherText = this.EncryptData(session, key, plainText, counter, nonce, null);


        using IMechanismParams salsaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20Params(counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20, salsaParams);

        byte[] newPlainText = session.Decrypt(mechanism, key, cipherText);

        Assert.IsTrue(newPlainText.SequenceEqual(plainText), $"Decryption error. Excepted: {Convert.ToHexString(plainText)}, actual: {Convert.ToHexString(newPlainText)}");
    }

    [TestMethod]
    [DataRow(0UL, 64, 256, 32)]
    [DataRow(0UL, 192, 256, 16)]
    //[DataRow(0, 192, 256, 122)]
    //[DataRow(51, 64, 87, 52)]
    //[DataRow(13, 96, 20786, 200)]
    //[DataRow(13, 192, 478, 32)]
    public void Decrypt_Salsa20WithEncriptBuffer_Success(ulong counter, int nonceBits, int plainTextLen, int streamBufferLen)
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

        byte[] cipherText = this.EncryptData(session, key, plainText, counter, nonce, streamBufferLen);

        using IMechanismParams salaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20Params(counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20, salaParams);

        byte[] newPlainText = session.Decrypt(mechanism, key, cipherText);

        Assert.IsTrue(newPlainText.SequenceEqual(plainText), $"Decryption error. Excepted: {Convert.ToHexString(plainText)}, actual: {Convert.ToHexString(newPlainText)}");
    }

    [TestMethod]
    [DataRow(0UL, 64, 256, 32)]
    [DataRow(0UL, 192, 256, 16)]
    //[DataRow(0, 192, 256, 122)]
    //[DataRow(51, 64, 87, 52)]
    //[DataRow(13, 96, 20786, 200)]
    //[DataRow(13, 192, 478, 32)]
    public void Decrypt_Salsa20WithBuffer_Success(ulong counter, int nonceBits, int plainTextLen, int streamBufferLen)
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

        byte[] cipherText = this.EncryptData(session, key, plainText, counter, nonce, null);


        using IMechanismParams salsaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20Params((uint)counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20, salsaParams);

        using MemoryStream cipherTextMs = new MemoryStream(cipherText, false);
        using MemoryStream plaintextMx = new MemoryStream();

        session.Decrypt(mechanism, key, cipherTextMs, plaintextMx, streamBufferLen);

        byte[] newPlainText = plaintextMx.ToArray();

        Assert.IsTrue(newPlainText.SequenceEqual(plainText), $"Decryption error. Excepted: {Convert.ToHexString(plainText)}, actual: {Convert.ToHexString(newPlainText)}");
    }

    //[TestMethod]
    //[DataRow(96, 256, 0)]
    //[DataRow(96, 256, 59)]
    //[DataRow(96, 217, 0)]
    //[DataRow(96, 217, 8)]
    //[DataRow(96, 43, 59)]
    //public void Decrypt_ChaCha20Polly_Success(int nonceBits, int plainTextLen, int aadDataLen)
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

    //    IObjectHandle key = this.GenerateSalsa20Key(session);

    //    byte[] nonce = new byte[nonceBits / 8];
    //    Random.Shared.NextBytes(nonce);

    //    byte[] cipherText;

    //    {
    //        using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20ChaCha20Polly1305Params(nonce, aadData);
    //        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20_POLY1305, chachaParams);

    //        cipherText = session.Encrypt(mechanism, key, plainText);
    //    }

    //    Assert.IsNotNull(cipherText);

    //    {
    //        using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20ChaCha20Polly1305Params(nonce, aadData);
    //        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20_POLY1305, chachaParams);

    //        byte[] newPlainText = session.Decrypt(mechanism, key, cipherText);

    //        Assert.IsTrue(newPlainText.SequenceEqual(plainText), $"Decryption error. Excepted: {Convert.ToHexString(plainText)}, actual: {Convert.ToHexString(newPlainText)}");
    //    }
    //}

    private byte[] EncryptData(ISession session, IObjectHandle key, byte[] plainText, ulong counter, byte[] nonce, int? bufferlength)
    {
        using IMechanismParams salsaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkSalsa20Params(counter, nonce);
        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_SALSA20, salsaParams);

        if (bufferlength.HasValue)
        {
            using MemoryStream plainTextMs = new MemoryStream(plainText, false);
            plainTextMs.Position = 0L;
            using MemoryStream cipherTextMs = new MemoryStream();
            session.Encrypt(mechanism, key, plainTextMs, cipherTextMs, bufferlength.Value);
            return cipherTextMs.ToArray();
        }
        else
        {
            return session.Encrypt(mechanism, key, plainText);
        }
    }

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
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
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