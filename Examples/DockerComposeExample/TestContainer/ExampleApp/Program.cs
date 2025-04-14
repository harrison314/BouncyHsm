using BouncyHsm.Client;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ExampleApp;

internal static class BchClient
{
    private static HttpClient httpClient = new HttpClient();

    private const string BouncyHsmEndpoint = "https://localhost:7007/";
    private const string BouncyHsmEndpointDockerVariable = "BOUNCY_HSM_HTTP";

    public static IBouncyHsmClient Client
    {
        get => new BouncyHsmClient(string.IsNullOrEmpty(Environment.GetEnvironmentVariable(BouncyHsmEndpointDockerVariable))
                       ? BouncyHsmEndpoint
                       : Environment.GetEnvironmentVariable(BouncyHsmEndpointDockerVariable)!,
                   httpClient);
    }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, Example app!");
        Console.WriteLine();

        // construct Pkcs11 lib location for use with "dotnet run"
        string contentDir = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
        string pkcs11Location = Path.Combine(contentDir, BouncyHsmPkcs11Paths.CurrentPlatformSpecificPkcs11Path);

        Console.WriteLine("Load PKCS#11 from: {0}", pkcs11Location);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            pkcs11Location,
            AppType.SingleThreaded);

        // Create first slot
        string LoginPin = "123456";
        string runId = Guid.NewGuid().ToString();
        CreateSlotResultDto information = await BchClient.Client.CreateSlotAsync(new CreateSlotDto()
        {
            Description = $"Integration Test Slot - {runId}",
            IsHwDevice = true,
            Token = new CreateTokenDto()
            {
                Label = $"IntegrationTestSlot-{runId}",
                SerialNumber = null,
                SimulateHwMechanism = true,
                SimulateHwRng = true,
                SimulateProtectedAuthPath = false,
                SimulateQualifiedArea = false,
                SpeedMode = SpeedMode.WithoutRestriction,
                SignaturePin = null,
                SoPin = "12345678",
                UserPin = LoginPin
            }
        });

        int SlotId = information.SlotId;
        string TokenSerialNumber = information.TokenSerialNumber;

        // Find slot
        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Single();

        // Show slot info
        ISlotInfo slotInfo = slot.GetSlotInfo();
        Console.WriteLine("Slot info:");
        Console.WriteLine(" FirmwareVersion: {0}", slotInfo.FirmwareVersion);
        Console.WriteLine(" HardwareVersion: {0}", slotInfo.HardwareVersion);
        Console.WriteLine(" ManufacturerId:  {0}", slotInfo.ManufacturerId);
        Console.WriteLine(" SlotDescription: {0}", slotInfo.SlotDescription);
        Console.WriteLine();

        // List mechanisms
        List<CKM> mechanisms = slot.GetMechanismList();

        Console.WriteLine("Mechanism list:");
        foreach (CKM mechanism in mechanisms)
        {
            if (Enum.IsDefined<CKM>(mechanism))
            {
                Console.WriteLine(" - {0}", mechanism);
            }
            else
            {
                Console.WriteLine(" - 0x{0:x8}", (uint)mechanism);
            }
        }

        // Remove slot
        await BchClient.Client.DeleteSlotAsync(SlotId);
    }
}