﻿using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T25_Decrypt
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Decrypt_AesEcb_Success()
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

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_ECB);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    [TestMethod]
    public void Decrypt_AesEcbWithParts_Success()
    {
        byte[] plainText = new byte[160];
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

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_ECB);
        using MemoryStream plainTextMs = new MemoryStream(plainText);
        using MemoryStream ciperTextMs = new MemoryStream();
        session.Encrypt(mechanism, key, plainTextMs, ciperTextMs, 32);

        ciperTextMs.Position = 0L;
        using MemoryStream decrypted = new MemoryStream();
        session.Decrypt(mechanism, key, ciperTextMs, decrypted, 32);

    }

    [DataTestMethod]
    [DataRow(CKM.CKM_AES_CBC)]
    [DataRow(CKM.CKM_AES_CBC_PAD)]
    [DataRow(CKM.CKM_AES_CFB1)]
    [DataRow(CKM.CKM_AES_CFB8)]
    [DataRow(CKM.CKM_AES_CFB64)]
    [DataRow(CKM.CKM_AES_CFB128)]
    [DataRow(CKM.CKM_AES_OFB)]
    [DataRow(CKM.CKM_AES_CTR)]
    [DataRow(CKM.CKM_AES_CTS)]
    public void Decrypt_AesWithIv_Success(CKM mechanismType)
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
        byte[] iv = session.GenerateRandom(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(mechanismType, iv);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    [TestMethod]
    public void Decrypt_AesGcm_Success()
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
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    [TestMethod]
    public void Decrypt_AesCcm_Success()
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
        byte[] nonce = session.GenerateRandom(8);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkCcmParams ccmParams = session.Factories.MechanismParamsFactory.CreateCkCcmParams((ulong)plainText.Length,
            nonce,
            null,
            16 * 8);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CCM, ccmParams);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    [TestMethod]
    public void Decrypt_RSAPkcs1_Success()
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

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
        byte[] cipherText = session.Encrypt(mechanism, publicKey, plainText);
        byte[] decrypted = session.Decrypt(mechanism, privateKey, cipherText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    [DataTestMethod]
    [DataRow(CKM.CKM_SHA_1, CKG.CKG_MGF1_SHA1)]
    [DataRow(CKM.CKM_SHA256, CKG.CKG_MGF1_SHA256)]
    [DataRow(CKM.CKM_SHA512, CKG.CKG_MGF1_SHA512)]
    public void Decrypt_RSAOAEP_Success(CKM hashAlg, CKG mgf)
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

        using ICkRsaPkcsOaepParams mechanismParams = session.Factories.MechanismParamsFactory.CreateCkRsaPkcsOaepParams(
            (ulong)hashAlg,
            (ulong)mgf,
            (ulong)CKZ.CKZ_DATA_SPECIFIED,
            null
        );

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS_OAEP, mechanismParams);
        byte[] chiperText = session.Encrypt(mechanism, publicKey, plainText);
        byte[] decrypted = session.Decrypt(mechanism, privateKey, chiperText);

        Assert.IsNotNull(decrypted);
        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
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
