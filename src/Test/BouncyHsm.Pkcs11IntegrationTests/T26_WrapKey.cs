using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T26_WrapKey
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKM.CKM_AES_CBC_PAD)]
    public void Wrap_AesWithIv_Success(CKM mechanismType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(mechanismType, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_AesGcm_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] nonce = Utils.GetRandomBytes(16);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkGcmParams gcmParams = session.Factories.MechanismParamsFactory.CreateCkGcmParams(nonce,
            (ulong)0,
            null,
            16 * 8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_GCM, gcmParams);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_AesGcmWithEc_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateNistP256(session);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] nonce = Utils.GetRandomBytes(16);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkGcmParams gcmParams = session.Factories.MechanismParamsFactory.CreateCkGcmParams(nonce,
            (ulong)0,
            null,
            16 * 8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_GCM, gcmParams);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_AesCcm_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] nonce = Utils.GetRandomBytes(8);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkCcmParams ccmParams = session.Factories.MechanismParamsFactory.CreateCkCcmParams((ulong)16,
            nonce,
            null,
            16 * 8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CCM, ccmParams);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_AesKeyWrapPad_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] nonce = Utils.GetRandomBytes(8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_WRAP_PAD);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(CKM.CKM_AES_CBC, 16)]
    [DataRow(CKM.CKM_AES_ECB, 0)]
    [DataRow(CKM.CKM_AES_OFB, 16)]
    [DataRow(CKM.CKM_AES_CFB64, 16)]
    [DataRow(CKM.CKM_AES_CFB8, 16)]
    [DataRow(CKM.CKM_AES_CFB128, 16)]
    [DataRow(CKM.CKM_AES_CFB1, 16)]
    public void Wrap_AesKeyUpadedSecret_Success(CKM aesMechanism, int ivLen)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        byte[] secretValue = Utils.GetRandomBytes(21);

        List<IObjectAttribute> secretAttribute = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"GSecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secretValue),
        };

        IObjectHandle secretKeyHandle = session.CreateObject(secretAttribute);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = (ivLen > 0)
            ? Utils.GetRandomBytes(ivLen)
            : Array.Empty<byte>();

        using IMechanism mechanism = (ivLen > 0)
            ? session.Factories.MechanismFactory.Create(aesMechanism, iv)
            : session.Factories.MechanismFactory.Create(aesMechanism);

        byte[] wrappedKey = session.WrapKey(mechanism, key, secretKeyHandle);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(CKM.CKM_AES_CBC, 16)]
    [DataRow(CKM.CKM_AES_ECB, 0)]
    [DataRow(CKM.CKM_AES_OFB, 16)]
    [DataRow(CKM.CKM_AES_CFB64, 16)]
    [DataRow(CKM.CKM_AES_CFB8, 16)]
    [DataRow(CKM.CKM_AES_CFB128, 16)]
    [DataRow(CKM.CKM_AES_CFB1, 16)]
    public void Wrap_AesKeyUpadedRsa_Success(CKM aesMechanism, int ivLen)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = (ivLen > 0)
            ? Utils.GetRandomBytes(ivLen)
            : Array.Empty<byte>();

        using IMechanism mechanism = (ivLen > 0)
            ? session.Factories.MechanismFactory.Create(aesMechanism, iv)
            : session.Factories.MechanismFactory.Create(aesMechanism);

        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_RSAPkcs1_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle aesKey = this.GenerateAesKey(session, 32);
        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
        byte[] wrappedKey = session.WrapKey(mechanism, publicKey, aesKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(CKM.CKM_SHA_1, CKG.CKG_MGF1_SHA1)]
    [DataRow(CKM.CKM_SHA256, CKG.CKG_MGF1_SHA256)]
    [DataRow(CKM.CKM_SHA512, CKG.CKG_MGF1_SHA512)]
    public void Wrap_RSAOAEP_Success(CKM hashAlg, CKG mgf)
    {
        byte[] plainText = new byte[64];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle aesKey = this.GenerateAesKey(session, 32);
        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        using ICkRsaPkcsOaepParams mechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams(
            (ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
            null
        );

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, mechanismParams);
        byte[] wrappedKey = session.WrapKey(mechanism, publicKey, aesKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(CKM.CKM_AES_CBC_PAD, "id-Ed25519")]
    [DataRow(CKM.CKM_AES_CBC_PAD, "id-Ed448")]
    public void Wrap_AesWithIvEdwards_Success(CKM mechanismType, string curveName)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        byte[] namedCurveOid = new Org.BouncyCastle.Asn1.DerPrintableString(curveName).GetEncoded();
        string label = $"EdKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
             factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurveOid),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism generateMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_EDWARDS_KEY_PAIR_GEN);
        session.GenerateKeyPair(generateMechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(mechanismType, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(CKM.CKM_AES_CBC_PAD, "06032B656E", DisplayName = "OID X25519")]
    [DataRow(CKM.CKM_AES_CBC_PAD, "06032B656F", DisplayName = "OID X448")]
    public void Wrap_AesWithIvMontgomery_Success(CKM mechanismType, string ecParamsHex)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        byte[] namedCurve = PkcsExtensions.HexConvertor.GetBytes(ecParamsHex);
        string label = $"X-KeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurve),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism generateMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_MONTGOMERY_KEY_PAIR_GEN);
        session.GenerateKeyPair(generateMechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(mechanismType, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_WithPermitedAlgorithm_Failed()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ALLOWED_MECHANISMS, new List<CKM>(){ CKM.CKM_AES_CBC, CKM.CKM_AES_ECB }),
        };

        using IMechanism generationMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        IObjectHandle key = session.GenerateKey(generationMechanism, keyAttributes);

        byte[] nonce = Utils.GetRandomBytes(16);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkGcmParams gcmParams = session.Factories.MechanismParamsFactory.CreateCkGcmParams(nonce,
            (ulong)0,
            null,
            16 * 8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_GCM, gcmParams);
        Pkcs11Exception ex = Assert.ThrowsExactly<Pkcs11Exception>(() => session.WrapKey(mechanism, key, privateKey));
        Assert.AreEqual(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED, ex.RV);
    }

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87))]
    public void Wrap_AesWithIvMlDsa_Success(uint parameterSet)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"MlDsa-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_PARAMETER_SET, parameterSet),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism generateMechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_DSA_KEY_PAIR_GEN);
        session.GenerateKeyPair(generateMechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S))]
    public void Wrap_AesWithIvSlhDsa_Success(uint parameterSet)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"SlhDsa-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_PARAMETER_SET, parameterSet),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism generateMechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_SLH_DSA_KEY_PAIR_GEN);
        session.GenerateKeyPair(generateMechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024))]
    public void Wrap_AesWithIvMlKem_Success(uint parameterSet)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"MlKem-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_PARAMETER_SET, parameterSet),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism generateMechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_KEM_KEY_PAIR_GEN);
        session.GenerateKeyPair(generateMechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_WithCorrectDeriveTemplate_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        List<IObjectAttribute> wrapTemplateAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
        };

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 }),
        };

        IObjectHandle handle = session.CreateObject(keyAttributes);

        List<IObjectAttribute> aesKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP_TEMPLATE, wrapTemplateAttributes),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism aesKeygenerationMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);
        IObjectHandle key = session.GenerateKey(aesKeygenerationMechanism, aesKeyAttributes);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] wrappedKey = session.WrapKey(mechanism, key, handle);

        Assert.IsNotNull(wrappedKey);
    }

    [TestMethod]
    public void Wrap_WithCorrectDeriveTemplate_Throws()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        List<IObjectAttribute> wrapTemplateAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
        };

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, new byte[] { 1, 4, 5, 8, 7, 4, 1, 5, 6, 3, 2, 5, 8, 5, 4, 5, 84, 6, 99, 12, 5, 241, 111, 123, 0, 0, 0, 7 }),
        };

        IObjectHandle handle = session.CreateObject(keyAttributes);

        List<IObjectAttribute> aesKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32)),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP_TEMPLATE, wrapTemplateAttributes),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism aesKeygenerationMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);
        IObjectHandle key = session.GenerateKey(aesKeygenerationMechanism, aesKeyAttributes);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.WrapKey(mechanism, key, handle));
        Assert.AreEqual(CKR.CKR_KEY_HANDLE_INVALID, ex.RV);
    }

    [TestMethod]
    public void WrapKey_ReadonlySession_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session, ckaToken:false);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, Utils.GetRandomBytes(32, true)),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        IObjectHandle key = session.GenerateKey(mechanism, keyAttributes);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism wrapMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] wrappedKey = session.WrapKey(wrapMechanism, key, privateKey);

        Assert.IsNotNull(wrappedKey);
    }

    private IObjectHandle GenerateAesKey(ISession session, int size)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }

    private (IObjectHandle privateKey, IObjectHandle publicKey) GenerateRsa(ISession session, bool ckaToken = true)
    {
        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, ckaToken),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODULUS_BITS, 2048),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PUBLIC_EXPONENT, new byte[] { 0x01, 0x00, 0x01 })
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, ckaToken),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        return (privateKey, publicKey);
    }

    private (IObjectHandle privateKey, IObjectHandle publicKey) GenerateNistP256(ISession session)
    {
        byte[] namedCurveOid = PkcsExtensions.HexConvertor.GetBytes("06082A8648CE3D030107");

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurveOid),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        return (privateKey, publicKey);
    }
}
