using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignHmac
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM.CKM_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM.CKM_SHA_1_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC, 14)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC, 1)]
    [DataRow(CKK.CKK_SHA256_HMAC, CKM.CKM_SHA256_HMAC, 64)]
    [DataRow(CKK.CKK_SHA512_HMAC, CKM.CKM_SHA512_HMAC, 64)]
    public void Sign_Hmac_Success(CKK type, CKM signatureMechanism, int size)
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

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);
        byte[] seecrit = this.GetSeecretKeyValue(session, handle);

        this.VerifySignature(signatureMechanism, seecrit, dataToSign, signature);

        session.DestroyObject(handle);
    }

    [DataTestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_1.CKM_SHA3_224_HMAC, 28)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_1.CKM_SHA3_256_HMAC, 32)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_1.CKM_SHA3_256_HMAC, 14)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_1.CKM_SHA3_256_HMAC, 1)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_1.CKM_SHA3_384_HMAC, 48)]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_1.CKM_SHA3_512_HMAC, 64)]
    public void Sign_HmacSha3_Success(CKK type, CKM signatureMechanism, int size)
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

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);
        this.GenerateSeecret(type, size, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);
        byte[] seecrit = this.GetSeecretKeyValue(session, handle);

        this.VerifySignature(signatureMechanism, seecrit, dataToSign, signature);

        session.DestroyObject(handle);
    }

    private void VerifySignature(CKM signatureMechanism, byte[] key, byte[] data, byte[] signature)
    {
        byte[]? dotnetSignature = signatureMechanism switch
        {
            CKM.CKM_SHA256_HMAC => HMACSHA256.HashData(key, data),
            CKM.CKM_SHA512_HMAC => HMACSHA512.HashData(key, data),
            CKM.CKM_SHA_1_HMAC => HMACSHA1.HashData(key, data),
            CKM.CKM_SHA384_HMAC => HMACSHA384.HashData(key, data),
            CKM_V3_1.CKM_SHA3_224_HMAC => null,
            CKM_V3_1.CKM_SHA3_256_HMAC => HMACSHA3_256.HashData(key, data),
            CKM_V3_1.CKM_SHA3_384_HMAC => HMACSHA3_384.HashData(key, data),
            CKM_V3_1.CKM_SHA3_512_HMAC => HMACSHA3_512.HashData(key, data),
            _ => throw new InvalidOperationException()
        };

        if (dotnetSignature != null)
        {
            Assert.AreEqual(Convert.ToHexString(dotnetSignature), Convert.ToHexString(signature));
        }
        else
        {
            this.TestContext!.WriteLine("Skip HMAC verification for {0}", signatureMechanism);
        }
    }

    private void GenerateSeecret(CKK type, int size, Pkcs11InteropFactories factories, ISession session, string label, byte[] ckId)
    {
        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, type),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_GENERIC_SECRET_KEY_GEN);
        _ = session.GenerateKey(mechanism, keyAttributes);
    }

    private IObjectHandle FindSeecretKey(ISession session, byte[] ckaId, string ckaLabel)
    {
        List<IObjectAttribute> searchTemplate = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, ckaLabel)
        };

        return session.FindAllObjects(searchTemplate).Single();
    }

    private byte[] GetSeecretKeyValue(ISession session, IObjectHandle handle)
    {
        return session.GetAttributeValue(handle, new List<CKA>() { CKA.CKA_VALUE })
            .Single()
            .GetValueAsByteArray();
    }
}