using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.Common;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T17_GenerateKeyPair
{
    [TestMethod]
    public void GenerateKeyPair_RSA_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
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
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    public void GenerateKeyPair_RSAX9_31_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
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

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_RSA_X9_31_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    [DataRow("06082A8648CE3D030107", DisplayName = "NIST P-256")]
    [DataRow("06052B81040023", DisplayName = "NIST P-521")]
    public void GenerateKeyPair_EC_Success(string ecParamsHex)
    {
        byte[] namedCurveOid = PkcsExtensions.HexConvertor.GetBytes(ecParamsHex);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurveOid),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
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

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    [DataRow("06082A8648CE3D030107", true, true, DisplayName = "NIST P-256 sensitive")]
    [DataRow("06082A8648CE3D030107", false, false, DisplayName = "NIST P-256")]
    public void GenerateKeyPair_ECWithSesnsitive_Success(string ecParamsHex, bool sensitive, bool alwaisSensitive)
    {
        byte[] namedCurveOid = PkcsExtensions.HexConvertor.GetBytes(ecParamsHex);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurveOid),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, sensitive),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: alwaisSensitive, neverExtractable: true);
    }

    [TestMethod]
    [DataRow("06082A8648CE3D030107", false, true, DisplayName = "NIST P-256 neverExtractavle")]
    [DataRow("06082A8648CE3D030107", true, false, DisplayName = "NIST P-256")]
    public void GenerateKeyPair_ECExtractable_Success(string ecParamsHex, bool extractable, bool neverExtractble)
    {
        byte[] namedCurveOid = PkcsExtensions.HexConvertor.GetBytes(ecParamsHex);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurveOid),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, extractable),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: neverExtractble);
    }

    [TestMethod]
    public void GenerateKeyPair_ECWithParams_Success()
    {
        Utils.EcdhData ecdhData = Utils.CreateEcdhParams();
        byte[] ecParams = ecdhData.X9Parameters.ToAsn1Object().GetEncoded();

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"ECKeyParamTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, ecParams),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
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

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    [DataRow("06032B6570", DisplayName = "OID Ed25519")]
    [DataRow("130A69642D45643235353139", DisplayName = "Name Ed25519")]
    [DataRow("06032B6571", DisplayName = "OID Ed448")]
    [DataRow("130869642D4564343438", DisplayName = "Name Ed448")]
    public void GenerateKeyPair_Edwards_Success(string ecParamsHex)
    {
        byte[] namedCurve = PkcsExtensions.HexConvertor.GetBytes(ecParamsHex);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"EDKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_EDWARDS_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    [DataRow("06032B656E", DisplayName = "OID X25519")]
    [DataRow("130969642D583235353139", DisplayName = "Name X25519")]
    [DataRow("06032B656F", DisplayName = "OID X448")]
    [DataRow("130769642D58343438", DisplayName = "Name X448")]
    public void GenerateKeyPair_Montgomery_Success(string ecParamsHex)
    {
        byte[] namedCurve = PkcsExtensions.HexConvertor.GetBytes(ecParamsHex);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_MONTGOMERY_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87))]
    public void GenerateKeyPair_MlDsa_Success(uint parameterSet)
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_DSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
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
    public void GenerateKeyPair_SlhDsa_Success(uint parameterSet)
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_SLH_DSA_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768))]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024, DisplayName = nameof(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024))]
    public void GenerateKeyPair_MlKem_Success(uint parameterSet)
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_KEM_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        this.PrivateKeyCheckValues(session, privateKey, alwaisSesitive: true, neverExtractable: true);
    }

    [TestMethod]
    public void GenerateKeyPair_ReadOnlySession_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
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
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);
    }

    [TestMethod]
    public void GenerateKeyPair_NotLoggedIn_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        Assert.IsTrue(session.GetSessionInfo().State is CKS.CKS_RW_PUBLIC_SESSION or CKS.CKS_RO_PUBLIC_SESSION, "The user must not be logged in for this test.");

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
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
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
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
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);
    }

    private void PrivateKeyCheckValues(ISession session,
        IObjectHandle privateKeyHandle,
        bool alwaisSesitive,
        bool neverExtractable)
    {
        List<CKA> attributesToRead = new List<CKA>()
        {
            CKA.CKA_ALWAYS_SENSITIVE,
            CKA.CKA_NEVER_EXTRACTABLE
        };

        List<IObjectAttribute> readAttributes = session.GetAttributeValue(privateKeyHandle, attributesToRead);

        Assert.AreEqual(alwaisSesitive, readAttributes[0].GetValueAsBool(), "Failed CKA_ALWAYS_SENSITIVE");
        Assert.AreEqual(neverExtractable, readAttributes[1].GetValueAsBool(), "Failed CKA_NEVER_EXTRACTABLE");
    }

    [TestMethod]
    [DataRow(1UL, new uint[] { 5 }, new uint[] { 4 }, 32UL)]
    [DataRow(1UL, new uint[] { 0x0f }, new uint[] { 0x0c }, 32UL)]
    [DataRow(1UL, new uint[] { 0x0a }, new uint[] { 0x08 }, 32UL)]
    [DataRow(2UL, new uint[] { 5, 5 }, new uint[] { 4, 1 }, 1024UL)]
    public void GenerateKeyPair_Hss_Success(ulong levels, uint[] lmsTypes, uint[] lmotsTypes, ulong expectedRemaining)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = T20_SignHss.LoadLibrary(factories);
        using ISession session = T20_SignHss.OpenLoggedSession(library);

        T20_SignHss.CreateHssKeyPair(factories, session, levels,
            lmsTypes.Select(t => (ulong)t).ToArray(),
            lmotsTypes.Select(t => (ulong)t).ToArray(),
            false,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        List<IObjectAttribute> privateAttributes = session.GetAttributeValue(privateKey, new List<CKA>()
        {
            CKA_V3_2.CKA_HSS_KEYS_REMAINING,
            CKA.CKA_SENSITIVE,
            CKA.CKA_EXTRACTABLE,
            CKA.CKA_COPYABLE,
        });
        Assert.AreEqual(expectedRemaining, privateAttributes[0].GetValueAsUlong());
        Assert.IsTrue(privateAttributes[1].GetValueAsBool(), "CKA_SENSITIVE");
        Assert.IsFalse(privateAttributes[2].GetValueAsBool(), "CKA_EXTRACTABLE");
        Assert.IsFalse(privateAttributes[3].GetValueAsBool(), "CKA_COPYABLE");

        List<IObjectAttribute> publicAttributes = session.GetAttributeValue(publicKey, new List<CKA>()
        {
            CKA_V3_2.CKA_HSS_LEVELS,
            CKA_V3_2.CKA_HSS_LMS_TYPE,
            CKA_V3_2.CKA_HSS_LMOTS_TYPE,
        });
        Assert.AreEqual(levels, publicAttributes[0].GetValueAsUlong());
        Assert.AreEqual((ulong)lmsTypes[0], publicAttributes[1].GetValueAsUlong());
        Assert.AreEqual((ulong)lmotsTypes[0], publicAttributes[2].GetValueAsUlong());

        // Pkcs11Interop maps CKR_ATTRIBUTE_SENSITIVE of a single attribute to CannotBeRead.
        IObjectAttribute valueAttribute = session.GetAttributeValue(privateKey, new List<CKA>() { CKA.CKA_VALUE })[0];
        Assert.IsTrue(valueAttribute.CannotBeRead, "CKA_VALUE must be sensitive.");
    }

    [TestMethod]
    public void GenerateKeyPair_HssExtractable_TemplateInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = T20_SignHss.LoadLibrary(factories);
        using ISession session = T20_SignHss.OpenLoggedSession(library);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => T20_SignHss.CreateHssKeyPair(factories, session, 1,
            new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 },
            new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 },
            false,
            out _,
            out _,
            extractable: true));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV);
    }

    [TestMethod]
    public void GenerateKeyPair_HssCountMismatch_TemplateInconsistent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = T20_SignHss.LoadLibrary(factories);
        using ISession session = T20_SignHss.OpenLoggedSession(library);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => T20_SignHss.CreateHssKeyPair(factories, session, 2,
            new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 },
            new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8, CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 },
            false,
            out _,
            out _));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.RV);
    }
}
