using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection.Emit;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignRsa
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
    [DataRow(CKM.CKM_SHA1_RSA_X9_31, false)]
    [DataRow(CKM_V3_1.CKM_SHA3_224_RSA_PKCS, false)]
    [DataRow(CKM_V3_1.CKM_SHA3_256_RSA_PKCS, false)]
    [DataRow(CKM_V3_1.CKM_SHA3_384_RSA_PKCS, false)]
    [DataRow(CKM_V3_1.CKM_SHA3_512_RSA_PKCS, false)]
    public void SignRsaPkcs_Call_Success(CKM mechnism, bool createPkcs1DigestInfo)
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

        Assert.IsNotNull(signature);
    }

    //[TestMethod]
    //public void SignRsa_X9_31_Success()
    //{
    //    byte[] dataToSign = new byte[32];
    //    Random.Shared.NextBytes(dataToSign);

    //    Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
    //    using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
    //        AssemblyTestConstants.P11LibPath,
    //        AppType.SingleThreaded);

    //    List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
    //    ISlot slot = slots.SelectTestSlot();

    //    using ISession session = slot.OpenSession(SessionType.ReadOnly);
    //    session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

    //    string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
    //    byte[] ckId = session.GenerateRandom(32);

    //    this.CreateRsaKeyPair(factories, slot, ckId, label);

    //    IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

    //    using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_X9_31);

    //    byte[] signature = session.Sign(mechanism, handle, dataToSign);

    //    Assert.IsNotNull(signature);
    //}

    [TestMethod]
    public void SignRsaPkcs_WithDotnetWerify_Success()
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

        using RSA rsaPubKey = this.ExportPublicKey(session, publicKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        bool verfied = rsaPubKey.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        Assert.IsTrue(verfied, "Signature inconsistent.");
    }

    [TestMethod]
    public void SignRsaPss_WithDotnetWerify_Success()
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

        using RSA rsaPubKey = this.ExportPublicKey(session, publicKey);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkRsaPkcsPssParams mParam = factories.MechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA256,
             (ulong)CKG.CKG_MGF1_SHA256,
             32);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_PSS, mParam);
        byte[] signature = session.Sign(mechanism, privateKey, hash);

        bool verfied = rsaPubKey.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

        Assert.IsTrue(verfied, "Signature inconsistent.");
    }

    [TestMethod]
    public void SignRsaPssSha256_WithDotnetWerify_Success()
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

        using RSA rsaPubKey = this.ExportPublicKey(session, publicKey);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkRsaPkcsPssParams mParam = factories.MechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM.CKM_SHA256,
             (ulong)CKG.CKG_MGF1_SHA256,
             32);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA256_RSA_PKCS_PSS, mParam);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        bool verfied = rsaPubKey.VerifyData(dataToSign, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

        Assert.IsTrue(verfied, "Signature inconsistent.");
    }

    [TestMethod]
    public void SignRsaPssSha3_256_WithDotnetWerify_Success()
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

        using RSA rsaPubKey = this.ExportPublicKey(session, publicKey);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkRsaPkcsPssParams mParam = factories.MechanismParamsFactory.CreateCkRsaPkcsPssParams((ulong)CKM_V3_1.CKM_SHA3_256,
             (ulong)CKG_V3_1.CKG_MGF1_SHA3_256,
             32);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_1.CKM_SHA3_256_RSA_PKCS_PSS, mParam);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        bool verfied = rsaPubKey.VerifyData(dataToSign, signature, HashAlgorithmName.SHA3_256, RSASignaturePadding.Pss);

        Assert.IsTrue(verfied, "Signature inconsistent.");
    }

    [TestMethod]
    public void SignRsa9796_Success()
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

        this.CreateRsaKeyPair(factories, slot, ckId, label);

        IObjectHandle handle = this.FindPrivateKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_9796);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);

        Assert.IsNotNull(signature);
    }

    private RSA ExportPublicKey(ISession session, IObjectHandle pubKeyHandle)
    {
        List<CKA> attributes = new List<CKA>()
        {
            CKA.CKA_MODULUS,
            CKA.CKA_PUBLIC_EXPONENT
        };

        List<IObjectAttribute> attrValues = session.GetAttributeValue(pubKeyHandle, attributes);

        RSA rsaPubKey = RSA.Create(new RSAParameters()
        {
            Modulus = attrValues[0].GetValueAsByteArray(),
            Exponent = attrValues[1].GetValueAsByteArray(),
        });

        return rsaPubKey;
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

    private (IObjectHandle publicKey, IObjectHandle privateKey) CreateRsaKeyPair(Pkcs11InteropFactories factories, ISlot slot, byte[] ckId, string label)
    {
        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        IObjectHandle publicKey, privateKey;
        CreateRsaKeyPair(factories, ckId, label, true, session, out publicKey, out privateKey);

        return (publicKey, privateKey);
    }

    private static void CreateRsaKeyPair(Pkcs11InteropFactories factories, byte[] ckId, string label, bool token, ISession session, out IObjectHandle publicKey, out IObjectHandle privateKey)
    {
        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
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

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
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
