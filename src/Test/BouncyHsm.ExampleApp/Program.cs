using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Text;

namespace BouncyHsm.ExampleApp;

public static class Program
{
    public const string P11LibPath = "BouncyHsm.Pkcs11Lib.dll";

    public const string UserPin = "123456";

    public static void Main(string[] args)
    {
        Console.WriteLine("Examle app for use BouncyHsm");
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();

        Environment.SetEnvironmentVariable("BOUNCY_HSM_CFG_STRING", "Server=127.0.0.1; Port=8765; LogTarget=Console; LogLevel=TRACE;");

        CrateObjectExample();
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
}