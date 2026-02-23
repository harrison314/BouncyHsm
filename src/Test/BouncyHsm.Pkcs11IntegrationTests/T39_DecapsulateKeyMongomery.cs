using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Org.BouncyCastle.Asn1;
using Pkcs11Interop.Ext;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T39_DecapsulateKeyMongomery
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow("id-X25519", 0, CKD.CKD_NULL, 0)]
    [DataRow("id-X25519", 0, CKD.CKD_NULL, 12)]
    [DataRow("id-X25519", 0, CKD.CKD_NULL, 16)]
    [DataRow("id-X25519", 0, CKD.CKD_NULL, 32)]
    [DataRow("id-X25519", 0, CKD.CKD_SHA1_KDF, 32)]
    [DataRow("id-X25519", 0, CKD.CKD_SHA224_KDF, 32)]
    [DataRow("id-X25519", 0, CKD.CKD_SHA256_KDF, 32)]
    [DataRow("id-X25519", 0, CKD.CKD_SHA512_KDF, 32)]
    [DataRow("id-X25519", 0, CKD.CKD_SHA384_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow("id-X25519", 0, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    [DataRow("id-X25519", 16, CKD.CKD_SHA1_KDF, 32)]
    [DataRow("id-X25519", 16, CKD.CKD_SHA224_KDF, 32)]
    [DataRow("id-X25519", 16, CKD.CKD_SHA256_KDF, 32)]
    [DataRow("id-X25519", 16, CKD.CKD_SHA512_KDF, 32)]
    [DataRow("id-X25519", 16, CKD.CKD_SHA384_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow("id-X25519", 16, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    [DataRow("id-X448", 0, CKD.CKD_NULL, 0)]
    [DataRow("id-X448", 0, CKD.CKD_NULL, 12)]
    [DataRow("id-X448", 0, CKD.CKD_NULL, 16)]
    [DataRow("id-X448", 0, CKD.CKD_NULL, 32)]
    [DataRow("id-X448", 0, CKD.CKD_SHA1_KDF, 32)]
    [DataRow("id-X448", 0, CKD.CKD_SHA224_KDF, 32)]
    [DataRow("id-X448", 0, CKD.CKD_SHA256_KDF, 32)]
    [DataRow("id-X448", 0, CKD.CKD_SHA512_KDF, 32)]
    [DataRow("id-X448", 0, CKD.CKD_SHA384_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow("id-X448", 0, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    [DataRow("id-X448", 16, CKD.CKD_SHA1_KDF, 32)]
    [DataRow("id-X448", 16, CKD.CKD_SHA224_KDF, 32)]
    [DataRow("id-X448", 16, CKD.CKD_SHA256_KDF, 32)]
    [DataRow("id-X448", 16, CKD.CKD_SHA512_KDF, 32)]
    [DataRow("id-X448", 16, CKD.CKD_SHA384_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_BLAKE2B_160_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_BLAKE2B_384_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_BLAKE2B_256_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_BLAKE2B_512_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_SHA3_256_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_SHA3_384_KDF, 32)]
    [DataRow("id-X448", 16, CKD_V3_0.CKD_SHA3_512_KDF, 32)]
    public void DecapsulateKey_EcDh1Derive_Success(string curveName, int sharedDataLength, CKD mgfFunction, int sectetLength)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GeneareMongomeryKeyPair(session, curveName);

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

        session.DecapsulateKey(library,
           mechanism,
           privateKey,
           template,
           cipherText,
           out IObjectHandle decapsulatedKey);

        this.AssertEqualSecret(secretKey, decapsulatedKey, session);
    }

    private (IObjectHandle privateKey, IObjectHandle publicKey) GeneareMongomeryKeyPair(ISession session,
       string curveName)
    {
        string label = $"X-KeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);
        byte[] namedCurve = new DerPrintableString(curveName).GetEncoded();

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurve),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_ENCAPSULATE, true),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_DECAPSULATE, true),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_MONTGOMERY_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        return (privateKey, publicKey);
    }

    private void AssertEqualSecret(IObjectHandle excepted, IObjectHandle actual, ISession session)
    {
        byte[] exceptedValue = session.GetAttributeValue(excepted, new List<CKA>() { CKA.CKA_VALUE })[0].GetValueAsByteArray();
        byte[] actualValue = session.GetAttributeValue(actual, new List<CKA>() { CKA.CKA_VALUE })[0].GetValueAsByteArray();

        Assert.IsTrue(exceptedValue.SequenceEqual(actualValue));
    }
}
