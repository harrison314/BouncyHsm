using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T25_DecryptErrorStates
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_SHA_1, CKG.CKG_MGF1_SHA1)]
    [DataRow(CKM.CKM_SHA256, CKG.CKG_MGF1_SHA256)]
    [DataRow(CKM.CKM_SHA512, CKG.CKG_MGF1_SHA512)]
    public void Decrypt_RSAOAEP_Cipher_Text_Too_long(CKM hashAlg, CKG mgf)
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

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        using ICkRsaPkcsOaepParams mechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams((ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
            null);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, mechanismParams);
        byte[] cipherText = session.Encrypt(mechanism, publicKey, plainText);

        // Create a larger array that is padded with an extra 0 byte
        byte[] modifiedCipherText = new byte[cipherText.Length + 1];
        cipherText.CopyTo(modifiedCipherText, 0);

        Pkcs11Exception e = Assert.ThrowsException<Pkcs11Exception>(() =>
        {
            session.Decrypt(mechanism, privateKey, modifiedCipherText);
        });

        Assert.AreEqual(CKR.CKR_ENCRYPTED_DATA_LEN_RANGE, e.RV);
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_SHA_1, CKG.CKG_MGF1_SHA1)]
    [DataRow(CKM.CKM_SHA256, CKG.CKG_MGF1_SHA256)]
    [DataRow(CKM.CKM_SHA512, CKG.CKG_MGF1_SHA512)]
    public void Decrypt_RSAOAEP_Cipher_Text_Modified(CKM hashAlg, CKG mgf)
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

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        using ICkRsaPkcsOaepParams mechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams((ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
            null);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, mechanismParams);
        byte[] cipherText = session.Encrypt(mechanism, publicKey, plainText);

        if (cipherText[^1] == 'a')
        {
            cipherText[^1] = (byte)'b';
        }
        else
        {
            cipherText[^1] = (byte)'a';
        }

        Pkcs11Exception e = Assert.ThrowsException<Pkcs11Exception>(() =>
        {
            session.Decrypt(mechanism, privateKey, cipherText);
        });

        Assert.AreEqual(CKR.CKR_ENCRYPTED_DATA_INVALID, e.RV);
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_SHA_1, CKG.CKG_MGF1_SHA1)]
    [DataRow(CKM.CKM_SHA256, CKG.CKG_MGF1_SHA256)]
    [DataRow(CKM.CKM_SHA512, CKG.CKG_MGF1_SHA512)]
    public void Decrypt_RSAOAEP_Cipher_Text_Modified_SourceData(CKM hashAlg, CKG mgf)
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

        (IObjectHandle privateKey, IObjectHandle publicKey) = this.GenerateRsa(session);

        // specify source data
        using ICkRsaPkcsOaepParams mechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams((ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
            new byte[] { 1, 2, 3, 4, 5, 6 });

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, mechanismParams);
        byte[] cipherText = session.Encrypt(mechanism, publicKey, plainText);

        // specify different source data for decryption
        using ICkRsaPkcsOaepParams invalidMechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams((ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
           new byte[] { 1, 2, 3, 4, 5 });
        using IMechanism invalidMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, invalidMechanismParams);

        Pkcs11Exception e = Assert.ThrowsException<Pkcs11Exception>(() =>
        {
            session.Decrypt(invalidMechanism, privateKey, cipherText);
        });

        Assert.AreEqual(CKR.CKR_ENCRYPTED_DATA_INVALID, e.RV);
    }

    [TestMethod]
    public void Decrypt_AesGcm_Modified_Cipher_Text()
    {
        byte[] plainText = new byte[16];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] nonce = session.GenerateRandom(16);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkGcmParams gcmParams = session.Factories.MechanismParamsFactory.CreateCkGcmParams(nonce,
            (ulong)0,
            null,
            16 * 8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_GCM, gcmParams);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);

        if (cipherText[^1] == 'a')
        {
            cipherText[^1] = (byte)'b';
        }
        else
        {
            cipherText[^1] = (byte)'a';
        }

        Pkcs11Exception e = Assert.ThrowsException<Pkcs11Exception>(() =>
        {
            session.Decrypt(mechanism, key, cipherText);
        });

        Assert.AreEqual(CKR.CKR_ENCRYPTED_DATA_INVALID, e.RV);
    }

    [TestMethod]
    public void Decrypt_AesGcm_Modified_AAD()
    {
        byte[] plainText = new byte[16];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] nonce = session.GenerateRandom(16);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkGcmParams gcmParams = session.Factories.MechanismParamsFactory.CreateCkGcmParams(nonce,
            (ulong)0,
            new byte[] { 1, 2, 3, 4, 5, 6 },
            16 * 8);


        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_GCM, gcmParams);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkGcmParams invalidGcmParams = session.Factories.MechanismParamsFactory.CreateCkGcmParams(nonce,
        (ulong)0,
        new byte[] { 1, 2, 3, 4, 5},
        16 * 8);

        using IMechanism invalidMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_GCM, invalidGcmParams);

        Pkcs11Exception e = Assert.ThrowsException<Pkcs11Exception>(() =>
        {
            session.Decrypt(invalidMechanism, key, cipherText);
        });

        Assert.AreEqual(CKR.CKR_ENCRYPTED_DATA_INVALID, e.RV);
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

    private IObjectHandle GenerateAesKey(ISession session, int size)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}