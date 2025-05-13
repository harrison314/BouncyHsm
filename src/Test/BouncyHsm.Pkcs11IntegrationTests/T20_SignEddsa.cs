using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Security.Cryptography;
using Pkcs11Interop.Ext;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignEddsa
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void SignEddsa_Ed25519Sign_Success()
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

        string label = $"EdKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateEcdsaKeyPair(factories,
            "id-Ed25519",
            ckId,
            label,
            false,
            session,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EDDSA);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        Assert.IsNotNull(signature);
    }

    [DataTestMethod]
    [DataRow("id-Ed25519", false, "-")]
    [DataRow("id-Ed25519", false, "")]
    [DataRow("id-Ed25519", true, "-")]
    [DataRow("id-Ed25519", true, "")]
    [DataRow("id-Ed25519", false, "46b980b8e99bcbb1f7dbcfbfadfbd26dfdabadcffef2312d3705ef4024a75d78")]
    [DataRow("id-Ed25519", true, "86f261d20e25c5284403de5e97525bf0")]
    [DataRow("id-Ed448", false, "-")]
    [DataRow("id-Ed448", false, "")]
    [DataRow("id-Ed448", true, "-")]
    [DataRow("id-Ed448", true, "")]
    [DataRow("id-Ed448", false, "46b980b8e99bcbb1f7dbcfbfadfbd26dfdabadcffef2312d3705ef4024a75d78")]
    [DataRow("id-Ed448", true, "86f261d20e25c5284403de5e97525bf0")]
    public void SignEddsa_WithParams_Success(string curveName, bool pfFlag, string contextData)
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

        string label = $"EdKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateEcdsaKeyPair(factories,
            curveName,
            ckId,
            label,
            false,
            session,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        byte[]? contextDataBytes = this.GetContextData(contextData);

        using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams.ICkEddsaParams edDsaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkEddsaParams(pfFlag, contextDataBytes);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EDDSA, edDsaParams);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        Assert.IsNotNull(signature);
    }

    private byte[]? GetContextData(string contextData)
    {
        if (contextData == "-")
        {
            return null;
        }

        if (string.IsNullOrEmpty(contextData))
        {
            return Array.Empty<byte>();
        }

        return Convert.FromHexString(contextData);
    }

    private static void CreateEcdsaKeyPair(Pkcs11InteropFactories factories, string curveName, byte[] ckId, string label, bool token, ISession session, out IObjectHandle publicKey, out IObjectHandle privateKey)
    {
        //NIST P-256
        byte[] namedCurveOid = new Org.BouncyCastle.Asn1.DerPrintableString(curveName).GetEncoded();

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
             factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
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

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_EDWARDS_KEY_PAIR_GEN);
        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out publicKey,
            out privateKey);
    }
}
