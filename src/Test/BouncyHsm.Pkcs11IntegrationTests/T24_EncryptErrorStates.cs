using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T24_EncryptErrorStates
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_SHA_1, CKG.CKG_MGF1_SHA1, 214)]
    [DataRow(CKM.CKM_SHA256, CKG.CKG_MGF1_SHA256, 190)]
    [DataRow(CKM.CKM_SHA512, CKG.CKG_MGF1_SHA512, 126)]
    public void Encrypt_RSAOAEP_Plain_Text_Too_Long(CKM hashAlg, CKG mgf, int maxLength)
    {
        // Add 1 byte to exceed RSA OAEP max length (max length assumes 2048-bit key is used)
        byte[] plainText = new byte[maxLength + 1];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        using ICkRsaPkcsOaepParams mechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams((ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
            null);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, mechanismParams);

        Pkcs11Exception e = Assert.ThrowsException<Pkcs11Exception>(() =>
        {
            session.Encrypt(mechanism, publicKey, plainText);
        });

        Assert.AreEqual(CKR.CKR_DATA_LEN_RANGE, e.RV);
    }

    private (IObjectHandle privateKey, IObjectHandle publicKey) GenerateRsa(ISession session)
    {
        string label = $"RSAKeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_MODULUS_BITS, 2048),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PUBLIC_EXPONENT, new byte[] { 0x01, 0x00, 0x01 })
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
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
}