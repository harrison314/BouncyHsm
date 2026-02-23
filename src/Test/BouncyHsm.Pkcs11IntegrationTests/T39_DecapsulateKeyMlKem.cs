using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T39_DecapsulateKeyMlKem
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024)]
    public void EncapsulateKey_MlKemToAes_Success(uint parameterSet)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        this.GenerateKeyPair(parameterSet, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        string label = $"Aes-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> template = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_AES),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_KEM);

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

    [TestMethod]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512, 0U)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768, 0U)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024, 0U)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512, 12U)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768, 12U)]
    [DataRow(Pkcs11Interop.Ext.Common.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024, 12U)]
    public void EncapsulateKey_MlKemToGenericSecret_Success(uint parameterSet, uint length)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        this.GenerateKeyPair(parameterSet, session, out IObjectHandle publicKey, out IObjectHandle privateKey);

        string label = $"Aes-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> template = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
        };

        if (length != 0)
        {
            template.Add(factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, length));
        }

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_KEM);

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

    private void AssertEqualSecret(IObjectHandle excepted, IObjectHandle actual, ISession session)
    {
        byte[] exceptedValue = session.GetAttributeValue(excepted, new List<CKA>() { CKA.CKA_VALUE })[0].GetValueAsByteArray();
        byte[] actualValue = session.GetAttributeValue(actual, new List<CKA>() { CKA.CKA_VALUE })[0].GetValueAsByteArray();

        Assert.IsTrue(exceptedValue.SequenceEqual(actualValue));
    }

    private void GenerateKeyPair(uint parameterSet,
        ISession session,
        out IObjectHandle publicKey,
        out IObjectHandle privateKey)
    {
        string label = $"MlKem-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_ENCAPSULATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_PARAMETER_SET, parameterSet),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
            session.Factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_DECAPSULATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false)
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_2.CKM_ML_KEM_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out publicKey,
            out privateKey);
    }
}
