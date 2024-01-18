using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Drawing;
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
        CreateSesitiveData();
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
}