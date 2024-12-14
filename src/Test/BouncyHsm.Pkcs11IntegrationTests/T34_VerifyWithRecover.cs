using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using PkcsExtensions;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T34_VerifyWithRecover
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void VerifyRecover_RsaPkcs1_Success()
    {
        byte[] dataToSign = new byte[152];
        Random.Shared.NextBytes(dataToSign);

        byte[] hash = SHA256.HashData(dataToSign);
        dataToSign = Utils.CreatePkcs1DigestInfo(hash, HashAlgorithmName.SHA256);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        this.CreateRsaKeyPair(factories, slot, ckId, label, false);

        IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);

        byte[] signature = session.SignRecover(mechanism, handle, dataToSign);

        IObjectHandle pubKey = this.FindPublicKey(session, ckId, label);

        byte[] recoveredData = session.VerifyRecover(mechanism, pubKey, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");
        Assert.IsTrue(recoveredData.SequenceEqual(dataToSign), $"Recovered data {HexConvertor.GetString(recoveredData)} does not match with data to sign {HexConvertor.GetString(dataToSign)}.");
    }

    [TestMethod]
    public void VerifyRecover_RsaPkcs1StandardSign_Success()
    {
        byte[] dataToSign = new byte[152];
        Random.Shared.NextBytes(dataToSign);

        byte[] hash = SHA256.HashData(dataToSign);
        dataToSign = Utils.CreatePkcs1DigestInfo(hash, HashAlgorithmName.SHA256);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        this.CreateRsaKeyPair(factories, slot, ckId, label, true);

        IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        IObjectHandle pubKey = this.FindPublicKey(session, ckId, label);

        byte[] recoveredData = session.VerifyRecover(mechanism, pubKey, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");
        Assert.IsTrue(recoveredData.SequenceEqual(dataToSign), $"Recovered data {HexConvertor.GetString(recoveredData)} does not match with data to sign {HexConvertor.GetString(dataToSign)}.");
    }


    [TestMethod]
    public void VerifyRecover_Rsa9796_Success()
    {
        byte[] dataToSign = new byte[32];
        Random.Shared.NextBytes(dataToSign);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        this.CreateRsaKeyPair(factories, slot, ckId, label, false);

        IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_9796);

        byte[] signature = session.SignRecover(mechanism, handle, dataToSign);


        IObjectHandle pubKey = this.FindPublicKey(session, ckId, label);

        byte[] recoveredData = session.VerifyRecover(mechanism, pubKey, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");
        Assert.IsTrue(recoveredData.SequenceEqual(dataToSign), $"Recovered data {HexConvertor.GetString(recoveredData)} does not match with data to sign {HexConvertor.GetString(dataToSign)}.");
    }

    [TestMethod]
    public void VerifyRecover_Rsa9796WithStanardSign_Success()
    {
        byte[] dataToSign = new byte[32];
        Random.Shared.NextBytes(dataToSign);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        this.CreateRsaKeyPair(factories, slot, ckId, label, true);

        IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_9796);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);


        IObjectHandle pubKey = this.FindPublicKey(session, ckId, label);

        byte[] recoveredData = session.VerifyRecover(mechanism, pubKey, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");
        Assert.IsTrue(recoveredData.SequenceEqual(dataToSign), $"Recovered data {HexConvertor.GetString(recoveredData)} does not match with data to sign {HexConvertor.GetString(dataToSign)}.");
    }

    private IObjectHandle FindPublicKey(ISession session, byte[] ckaId, string ckaLabel)
    {
        List<IObjectAttribute> searchTemplate = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_RSA),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, ckaLabel)
        };

        return session.FindAllObjects(searchTemplate).Single();
    }

    private IObjectHandle FindPrivateKey(ISession session, byte[] ckaId, string ckaLabel)
    {
        List<IObjectAttribute> searchTemplate = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_RSA),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, ckaLabel)
        };

        return session.FindAllObjects(searchTemplate).Single();
    }

    private (IObjectHandle publicKey, IObjectHandle privateKey) CreateRsaKeyPair(Pkcs11InteropFactories factories, ISlot slot, byte[] ckId, string label, bool enableStandardSign)
    {
        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        IObjectHandle publicKey, privateKey;
        CreateRsaKeyPair(factories, ckId, label, true, session, enableStandardSign, out publicKey, out privateKey);

        return (publicKey, privateKey);
    }

    private static void CreateRsaKeyPair(Pkcs11InteropFactories factories, byte[] ckId, string label, bool token, ISession session, bool enableStandardSign, out IObjectHandle publicKey, out IObjectHandle privateKey)
    {
        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODULUS_BITS, 2048),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PUBLIC_EXPONENT, new byte[] { 0x01, 0x00, 0x01 })
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, enableStandardSign),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);
        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out publicKey,
            out privateKey);
    }
}