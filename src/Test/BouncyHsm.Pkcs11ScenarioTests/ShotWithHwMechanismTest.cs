using BouncyHsm.Client;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11ScenarioTests;

[TestClass]
public sealed class ShotWithHwMechanismTest
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

    [ClassInitialize]
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
                IsRemovableDevice = false,
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
                    UserPin = LoginPin,
                }
            });

            SlotId = information.SlotId;
            TokenSerialNumber = information.TokenSerialNumber;
        }
        catch (ApiBouncyHsmException<ProblemDetails> ex)
        {
            testContext.WriteLine(ex.Result.Detail);
            testContext.WriteLine(ex.ToString());
            throw;
        }
        catch (Exception ex)
        {
            testContext.WriteLine(ex.ToString());
            throw;
        }
    }

    [ClassCleanup]
    public static async Task Cleanup()
    {
        if (SlotId.HasValue)
        {
            await BchClient.Client.DeleteSlotAsync(SlotId.Value);
        }
    }


    [TestMethod]
    public void Slot_HwMechanisms_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BchClient.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Single();

        List<CKM> list = slot.GetMechanismList();

        foreach (CKM mechanism in list)
        {
            IMechanismInfo info = slot.GetMechanismInfo(mechanism);
            Assert.IsTrue(info.MechanismFlags.Hw);
        }
    }
}
