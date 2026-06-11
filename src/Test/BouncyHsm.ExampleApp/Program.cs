using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using Pkcs11Interop.Ext;
using Pkcs11Interop.Ext.Common;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace BouncyHsm.ExampleApp;

public static class Program
{
    public static string P11LibPath
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "BouncyHsm.Pkcs11Lib.dll";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "./BouncyHsm.Pkcs11Lib-x64.so";
            }

            throw new PlatformNotSupportedException();
        }
    }

    public const string UserPin = "123456";

    public static void Main(string[] args)
    {
        Console.WriteLine("Examle app for use BouncyHsm");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();

        Environment.SetEnvironmentVariable("BOUNCY_HSM_CFG_STRING", "Server=127.0.0.1; Port=8765; LogTarget=Console; LogLevel=TRACE; Tag=MyExampleApp;");

        //CrateObjectExample();
        //EncryptAndDecrypt();
        // CreateSesitiveData();
        //ChaCha20();
        //CreateAndReadRsa();
        //AttributeTemplates();
        SampleCounterModeSp800_108();
    }

    private static void CrateObjectExample()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);


        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_DATA),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, "MyObject"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Encoding.UTF8.GetBytes("Hello wold!")),
        };

        _ = session.CreateObject(objectAttributes);
    }

    private static void EncryptAndDecrypt()
    {
        byte[] plainText = new byte[16 * 8];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);


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
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism keyGenMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);
        IObjectHandle key = session.GenerateKey(keyGenMechanism, keyAttributes);

        byte[] iv = session.GenerateRandom(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);
    }

    private static void CreateSesitiveData()
    {

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);


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
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism keyGenMechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);
        IObjectHandle key = session.GenerateKey(keyGenMechanism, keyAttributes);

        List<CKA> listAttr = new List<CKA>()
        {
         CKA.CKA_LABEL,
         CKA.CKA_ID,
         CKA.CKA_CLASS,
         CKA.CKA_KEY_TYPE,
         CKA.CKA_EXTRACTABLE
         };

        List<IObjectAttribute> attributes = session.GetAttributeValue(key, listAttr);

        Console.WriteLine("Key is sensitive {0}", attributes[4].GetValueAsBool());
    }

    private static void ChaCha20()
    {

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);


        string label = $"ChaCha20-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32U),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20_KEY_GEN);

        IObjectHandle keyHandle = session.GenerateKey(mechanism, keyAttributes);

        byte[] plainText = Encoding.UTF8.GetBytes("Hello world! This is a test of ChaCha20 encryption.");


        byte[] nonce = new byte[8];
        Random.Shared.NextBytes(nonce);
        using IMechanismParams chachaParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateCkChaCha20Params((uint)0, nonce);
        using IMechanism chacha20mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_CHACHA20, chachaParams);

        byte[] cipherText = session.Encrypt(chacha20mechanism, keyHandle, plainText);

        Console.WriteLine("CipherText: {0}", Convert.ToBase64String(cipherText));
    }

    private static void CreateAndReadRsa()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);

        string label = $"RSAKeyTest-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

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


        List<IObjectAttribute> attributes = session.GetAttributeValue(publicKey, new List<CKA>()
        {
            CKA.CKA_CLASS, CKA.CKA_TOKEN, CKA. CKA_PRIVATE, CKA.CKA_MODIFIABLE, CKA. CKA_LABEL,
            CKA.CKA_ID, CKA. CKA_START_DATE, CKA. CKA_END_DATE, CKA.CKA_DERIVE, CKA.CKA_LOCAL,
            CKA.CKA_ALLOWED_MECHANISMS, CKA.CKA_SUBJECT, CKA. CKA_ENCRYPT, CKA.CKA_VERIFY,
            CKA.CKA_VERIFY_RECOVER, CKA.CKA_WRAP, CKA.CKA_TRUSTED, CKA.CKA_WRAP_TEMPLATE,
            CKA.CKA_KEY_TYPE, CKA.CKA_KEY_GEN_MECHANISM, CKA.CKA_MODULUS, CKA.CKA_MODULUS_BITS,
            CKA.CKA_PUBLIC_EXPONENT
        });

        byte[] publicExponent = attributes.Last().GetValueAsByteArray();
        Console.WriteLine("Public exponent: {0}", Convert.ToHexString(publicExponent));
    }

    private static void AttributeTemplates()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> deriveTemplate = new List<IObjectAttribute>()
        {
             factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
             factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
             factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 16),
             factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
             factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
             factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
             factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, true),
             factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
             factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            //    CKA_ALLOWED_MECHANISMS(), att(arena, CK_MECHANISM_TYPE, CKM_CONCATENATE_BASE_AND_DATA()),
            //    CKA_DERIVE_TEMPLATE(), concatTemplate,
        };

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, session.GenerateRandom(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE_TEMPLATE, deriveTemplate),
        };

        IObjectHandle secretKey = session.CreateObject(keyAttributes);

        List<IObjectAttribute> result = session.GetAttributeValue(secretKey, new List<CKA>()
        {
            CKA.CKA_ID,
            CKA.CKA_ENCRYPT,
            CKA.CKA_DECRYPT,
            CKA.CKA_DERIVE_TEMPLATE,
            CKA.CKA_MODIFIABLE
        });

        List<IObjectAttribute> deriveTemplateReaded = result[3].GetValueAsObjectAttributeList();

        foreach (IObjectAttribute attr in deriveTemplateReaded)
        {
            Console.WriteLine((CKA)attr.Type);
        }
    }

    private static void SampleCounterModeSp800_108()
    {
        // PRF (KI, [i]2 || Label || 0x00 || Context || [L]2)
        // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sp800108hmaccounterkdf?view=net-10.0
        // https://docs.oasis-open.org/pkcs11/pkcs11-spec/v3.2/pkcs11-spec-v3.2.html#_Toc195693562
        string keyValue = "26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d";
        string labelValue = "99c3d79cb978724e1e2f09dc90e3b694";
        string contextValue = "18582cd847d60455fb88924c9fd8fb63";

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.First();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, UserPin);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, Convert.FromHexString(keyValue)),
        };

        IObjectHandle secretKey = session.CreateObject(keyAttributes);

        List<IObjectAttribute> deriveTemplate = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}"),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, session.GenerateRandom(32)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)64),
        };

        using ICkSP800_108KdfParams derivationMechanismParams = Pkcs11V3_0Factory.Instance.MechanismParamsFactory.CreateSp800_108KdfParams(CKM.CKM_SHA256_HMAC,
             null,
            new KdfDataParam.IterationVariable(false, 32),
            new KdfDataParam.ByteArray(Convert.FromHexString(labelValue)),
            new KdfDataParam.ByteArray(new byte[] { 0x00 }),
            new KdfDataParam.ByteArray(Convert.FromHexString(contextValue)),
            new KdfDataParam.DkmLength(false, 32, CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS));

        using IMechanism derivationMechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_SP800_108_COUNTER_KDF, derivationMechanismParams);

        IObjectHandle derivedKey = session.DeriveKey(derivationMechanism,
            secretKey,
            deriveTemplate);

        List<IObjectAttribute> derivedKeyAttrs = session.GetAttributeValue(derivedKey, new List<CKA>() { CKA.CKA_VALUE });
        byte[] derivedKeyValue = derivedKeyAttrs[0].GetValueAsByteArray();

        Console.WriteLine(Convert.ToHexString(derivedKeyValue));
    }
}