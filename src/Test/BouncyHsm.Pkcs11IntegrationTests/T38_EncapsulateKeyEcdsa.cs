using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T38_EncapsulateKeyEcdsa
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(0, CKD.CKD_NULL, 0)]
    [DataRow(0, CKD.CKD_NULL, 12)]
    [DataRow(0, CKD.CKD_NULL, 16)]
    [DataRow(0, CKD.CKD_NULL, 32)]
    [DataRow(0, CKD.CKD_SHA1_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA224_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA256_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA512_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA384_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA1_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA224_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA256_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA512_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA384_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    public void EncapsulateKey_EcDh1Derive_Success(int sharedDataLength, CKD mgfFunction, int sectetLength)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateNistP521(session);

        string label = $"Secret-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> template = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
        };

        if (sectetLength > 0)
        {
            template.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)sectetLength));
        }

        byte[]? sharedData = null;
        if (sharedDataLength > 0)
        {
            sharedData = session.GenerateRandom(sharedDataLength);
        }

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)mgfFunction,
            sharedData,
            null);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        session.EncapsulateKey(library,
            mechanism,
            publicKey,
            template,
            out byte[] cipherText,
            out IObjectHandle secretKey);

        Assert.IsNotNull(cipherText);
        Assert.AreNotEqual(0, cipherText.Length);
        Assert.IsNotNull(secretKey);
    }

    [TestMethod]
    [DataRow(0, CKD.CKD_NULL, 0)]
    [DataRow(0, CKD.CKD_NULL, 12)]
    [DataRow(0, CKD.CKD_NULL, 16)]
    [DataRow(0, CKD.CKD_NULL, 18)]
    [DataRow(0, CKD.CKD_SHA1_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA224_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA256_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA512_KDF, 32)]
    [DataRow(0, CKD.CKD_SHA384_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow(0, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA1_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA224_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA256_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA512_KDF, 32)]
    [DataRow(16, CKD.CKD_SHA384_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow(16, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    public void EncapsulateKey_EcDh1CofactorDerive_Success(int sharedDataLength, CKD mgfFunction, int sectetLength)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateNistP521(session);

        string label = $"Secret-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> template = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
        };

        if (sectetLength > 0)
        {
            template.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)sectetLength));
        }

        byte[]? sharedData = null;
        if (sharedDataLength > 0)
        {
            sharedData = session.GenerateRandom(sharedDataLength);
        }

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)mgfFunction,
            sharedData,
            null);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_ECDH1_COFACTOR_DERIVE, mp);

        session.EncapsulateKey(library,
            mechanism,
            publicKey,
            template,
            out byte[] cipherText,
            out IObjectHandle secretKey);

        Assert.IsNotNull(cipherText);
        Assert.AreNotEqual(0, cipherText.Length);
        Assert.IsNotNull(secretKey);
    }

    private (IObjectHandle privateKey, IObjectHandle publicKey) GenerateNistP521(ISession session)
    {
        byte[] namedCurveOid = (new Org.BouncyCastle.Asn1.DerObjectIdentifier("1.3.132.0.35").GetDerEncoded());

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurveOid),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_ENCAPSULATE, true),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_DECAPSULATE, true),
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
