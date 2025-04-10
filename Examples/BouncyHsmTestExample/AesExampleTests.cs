using BouncyHsm.Client;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BouncyHsmTestExample;

internal static class BchClient
{
    private static HttpClient httpClient = new HttpClient();

    private const string BouncyhsmEndpoint = "https://localhost:7007/";

    public static IBouncyHsmClient Client
    {
        get => new BouncyHsmClient(BouncyhsmEndpoint, httpClient);
    }
}

[TestClass]
public class InitializerTest
{
    public static int? SlotId
    {
        get;
        private set;
    }

    public static string? TokenSerialNumber
    {
        get;
        private set;
    }

    public static string LoginPin
    {
        get;
        private set;
    } = default!;

    [AssemblyInitialize]
    public static async Task Initialize(TestContext testContext)
    {
        LoginPin = "123456";
        try
        {
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

            SlotId = information.SlotId;
            TokenSerialNumber = information.TokenSerialNumber;
        }
        catch (Exception ex)
        {
            testContext.WriteLine(ex.ToString());
            throw;
        }
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        if (SlotId.HasValue)
        {
            await BchClient.Client.DeleteSlotAsync(SlotId.Value);
        }
    }
}

[TestClass]
public class AesExampleTests
{
    [TestMethod]
    public async Task AesExample_Encrypt()
    {
        (string label, byte[] ckId) = await this.InitializeAesKey();

        byte[] plainText = new byte[16 * 8];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BouncyHsmPkcs11Paths.CurrentPlatformSpecificPkcs11Path,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.Where(t => t.GetTokenInfo().SerialNumber == InitializerTest.TokenSerialNumber).Single();


        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(Net.Pkcs11Interop.Common.CKU.CKU_USER, InitializerTest.LoginPin);


        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, Net.Pkcs11Interop.Common.CKO.CKO_SECRET_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, Net.Pkcs11Interop.Common.CKK.CKK_AES),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
        };

        IObjectHandle key = session.FindAllObjects(keyAttributes).Single();

        byte[] iv = session.GenerateRandom(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC_PAD, iv);
        byte[] cipherText = session.Encrypt(mechanism, key, plainText);
        byte[] decrypted = session.Decrypt(mechanism, key, cipherText);

        Assert.AreEqual(Convert.ToHexString(plainText), Convert.ToHexString(decrypted));
    }

    private async Task<(string label, byte[] ckId)> InitializeAesKey()
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = RandomNumberGenerator.GetBytes(32);

        _ = await BchClient.Client.GenerateAesKeyAsync(InitializerTest.SlotId!.Value, new GenerateAesKeyRequestDto()
        {
            Size = 32,
            KeyAttributes = new GenerateKeyAttributesDto()
            {
                CkaId = ckId,
                CkaLabel = label,
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = false,
                ForWrap = false,
                Sensitive = true
            }
        });

        return (label, ckId);
    }
}