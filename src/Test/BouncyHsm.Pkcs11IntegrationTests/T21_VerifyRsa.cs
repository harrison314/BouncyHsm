using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Security.Cryptography;
using System.Reflection.Emit;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T21_VerifyRsa
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_RSA_PKCS, true)]
    [DataRow(CKM.CKM_SHA1_RSA_PKCS, false)]
    [DataRow(CKM.CKM_SHA224_RSA_PKCS, false)]
    [DataRow(CKM.CKM_SHA256_RSA_PKCS, false)]
    [DataRow(CKM.CKM_SHA384_RSA_PKCS, false)]
    [DataRow(CKM.CKM_SHA512_RSA_PKCS, false)]
    [DataRow(CKM.CKM_MD2_RSA_PKCS, false)]
    [DataRow(CKM.CKM_MD5_RSA_PKCS, false)]
    [DataRow(CKM.CKM_RIPEMD128_RSA_PKCS, false)]
    [DataRow(CKM.CKM_RIPEMD160_RSA_PKCS, false)]
    public void VerifyRsaPkcs_SignAndVerify_Success(CKM mechnism, bool createPkcs1DigestInfo)
    {
        byte[] dataToSign = new byte[412];
        Random.Shared.NextBytes(dataToSign);

        if (createPkcs1DigestInfo)
        {
            byte[] hash = SHA256.HashData(dataToSign);
            dataToSign = Utils.CreatePkcs1DigestInfo(hash, HashAlgorithmName.SHA256);
        }

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

        this.CreateRsaKeyPair(factories, slot, ckId, label);

        IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(mechnism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);


        IObjectHandle pubKey = this.FindPublicKey(session, ckId, label);

        session.Verify(mechanism, pubKey, dataToSign, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");

        unchecked
        {
            signature[2]++;
        }

        session.Verify(mechanism, pubKey, dataToSign, signature, out bool isNotValid);
        Assert.IsFalse(isNotValid, "Inconsistent signature is valid");
    }

    [TestMethod]
    public void VerifyRsaPkcs_SignAndVerify_Success()
    {
        byte[] dataToSign = new byte[412];
        Random.Shared.NextBytes(dataToSign);
        byte[] hash = SHA256.HashData(dataToSign);
        dataToSign = Utils.CreatePkcs1DigestInfo(hash, HashAlgorithmName.SHA256);


        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateRsaKeyPair(factories, ckId, label, false, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);


        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");

        unchecked
        {
            signature[2]++;
        }

        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isNotValid);
        Assert.IsFalse(isNotValid, "Inconsistent signature is valid");
    }

    [TestMethod]
    public void VerifyRsaPss_SignAndVerify_Success()
    {
        byte[] dataToSign = new byte[412];
        Random.Shared.NextBytes(dataToSign);
        byte[] hash = SHA256.HashData(dataToSign);


        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateRsaKeyPair(factories, ckId, label, false, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkRsaPkcsPssParams mParam = factories.MechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA256,
             (ulong)CKG.CKG_MGF1_SHA256,
             32);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_PSS, mParam);
        byte[] signature = session.Sign(mechanism, privateKey, hash);

        session.Verify(mechanism, publicKey, hash, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");

        unchecked
        {
            signature[2]++;
        }

        session.Verify(mechanism, publicKey, hash, signature, out bool isNotValid);
        Assert.IsFalse(isNotValid, "Inconsistent signature is valid");
    }

    [TestMethod]
    public void VerifyRsaPssSha256_SignAndVerify_Success()
    {
        byte[] dataToSign = new byte[412];
        Random.Shared.NextBytes(dataToSign);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateRsaKeyPair(factories, ckId, label, false, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkRsaPkcsPssParams mParam = factories.MechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA256,
             (ulong)CKG.CKG_MGF1_SHA256,
             32);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256_RSA_PKCS_PSS, mParam);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isValid);

        Assert.IsTrue(isValid, "Signature must by valid");

        unchecked
        {
            signature[2]++;
        }

        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isNotValid);
        Assert.IsFalse(isNotValid, "Inconsistent signature is valid");
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

    private (IObjectHandle publicKey, IObjectHandle privateKey) CreateRsaKeyPair(Pkcs11InteropFactories factories, ISlot slot, byte[] ckId, string label)
    {
        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        IObjectHandle publicKey, privateKey;
        CreateRsaKeyPair(factories, ckId, label, true, session, out publicKey, out privateKey);

        return (publicKey, privateKey);
    }

    private static void CreateRsaKeyPair(Pkcs11InteropFactories factories, byte[] ckId, string label, bool token, ISession session, out IObjectHandle publicKey, out IObjectHandle privateKey)
    {
        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODULUS_BITS, 2048),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PUBLIC_EXPONENT, new byte[] { 0x01, 0x00, 0x01 })
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);
        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out publicKey,
            out privateKey);
    }
}
