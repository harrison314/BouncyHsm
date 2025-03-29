using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignEcdsa
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void SignEcdsa_WithDotnetWerify_Success()
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

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateEcdsaKeyPair(factories, ckId, label, false, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using ECDsa rsaPubKey = this.ExportPublicKey(session, publicKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA);
        byte[] signature = session.Sign(mechanism, privateKey, hash);

        bool verfied = rsaPubKey.VerifyHash(hash, signature, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        Assert.IsTrue(verfied, "Signature inconsistent.");
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_ECDSA_SHA1)]
    [DataRow(CKM.CKM_ECDSA_SHA224)]
    [DataRow(CKM.CKM_ECDSA_SHA256)]
    [DataRow(CKM.CKM_ECDSA_SHA384)]
    [DataRow(CKM.CKM_ECDSA_SHA512)]
    public void SignEcdsaWithHash_WithDotnetWerify_Success(CKM signMechanism)
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

        string label = $"ECKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        CreateEcdsaKeyPair(factories, ckId, label, false, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using ECDsa rsaPubKey = this.ExportPublicKey(session, publicKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(signMechanism);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        if (signMechanism == CKM.CKM_ECDSA_SHA1)
        {
            bool verfied = rsaPubKey.VerifyData(dataToSign, signature, HashAlgorithmName.SHA1, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            Assert.IsTrue(verfied, "Signature inconsistent.");
        }
        else if (signMechanism == CKM.CKM_ECDSA_SHA384)
        {
            bool verfied = rsaPubKey.VerifyData(dataToSign, signature, HashAlgorithmName.SHA384, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            Assert.IsTrue(verfied, "Signature inconsistent.");
        }
        else if (signMechanism == CKM.CKM_ECDSA_SHA512)
        {
            bool verfied = rsaPubKey.VerifyData(dataToSign, signature, HashAlgorithmName.SHA512, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            Assert.IsTrue(verfied, "Signature inconsistent.");
        }
        else
        {
            this.TestContext?.WriteLine($"Skipped verify data for {signMechanism}.");
        }
    }

    [TestMethod]
    public void SignEcdsa_WithExplicitEcParams_Success()
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

        string label = $"Ec-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        Utils.EcdhData ecdhData = Utils.CreateEcdhParams();

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_ECDSA),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, ecdhData.X9Parameters.ToAsn1Object().GetEncoded()),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, ecdhData.EcPrivateKey.D.ToByteArrayUnsigned())
        };

        IObjectHandle privateKey = session.CreateObject(privateKeyAttributes);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA);
        byte[] signature = session.Sign(mechanism, privateKey, hash);

        using System.Security.Cryptography.X509Certificates.X509Certificate2 certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(ecdhData.RawCertificate);
        bool verfied = certificate.PublicKey.GetECDsaPublicKey()!.VerifyHash(hash, signature, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        Assert.IsTrue(verfied, "Signature inconsistent.");
    }

    [TestMethod]
    public void SignEcdsa_WithGeneratedExplicitEcParams_Success()
    {
        byte[] dataToSign = new byte[412];
        Random.Shared.NextBytes(dataToSign);
        byte[] hash = SHA256.HashData(dataToSign);

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
        byte[] ckId = session.GenerateRandom(32);

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

        using IMechanism signMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA);
        byte[] signature = session.Sign(signMechanism, privateKey, hash);
        Assert.IsNotNull(signature);
    }

    private ECDsa ExportPublicKey(ISession session, IObjectHandle pubKeyHandle)
    {
        List<CKA> attributes = new List<CKA>()
        {
            CKA.CKA_EC_POINT
        };

        List<IObjectAttribute> attrValues = session.GetAttributeValue(pubKeyHandle, attributes);
        byte[] ecPointEncoded = attrValues[0].GetValueAsByteArray();

        byte[] publicKeyBytes = Org.BouncyCastle.Asn1.DerOctetString.GetInstance(ecPointEncoded).GetOctets();

        Assert.AreEqual((byte)0x04, publicKeyBytes[0], "EC point data must start with 0x04 as uncmpressed curve point.");
        int half = (publicKeyBytes.Length - 1) / 2 + 1;

        ECDsa ecPubKey = ECDsa.Create(new ECParameters()
        {
            Curve = ECCurve.NamedCurves.nistP256,
            D = null,
            Q = new ECPoint()
            {
                X = publicKeyBytes[1..half],
                Y = publicKeyBytes[half..]
            }
        });

        return ecPubKey;
    }
    private IObjectHandle FindPrivateKey(ISession session, byte[] ckaId, string ckaLabel)
    {
        List<IObjectAttribute> searchTemplate = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_EC),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, ckaLabel)
        };

        return session.FindAllObjects(searchTemplate).Single();
    }

    private (IObjectHandle publicKey, IObjectHandle privateKey) CreateEcdsaKeyPair(Pkcs11InteropFactories factories, ISlot slot, byte[] ckId, string label)
    {
        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        IObjectHandle publicKey, privateKey;
        CreateEcdsaKeyPair(factories, ckId, label, true, session, out publicKey, out privateKey);

        return (publicKey, privateKey);
    }

    private static void CreateEcdsaKeyPair(Pkcs11InteropFactories factories, byte[] ckId, string label, bool token, ISession session, out IObjectHandle publicKey, out IObjectHandle privateKey)
    {
        //NIST P-256
        byte[] namedCurveOid = PkcsExtensions.HexConvertor.GetBytes("06082A8648CE3D030107");

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

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);
        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out publicKey,
            out privateKey);
    }
}