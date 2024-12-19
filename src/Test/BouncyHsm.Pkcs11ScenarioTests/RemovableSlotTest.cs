using BouncyHsm.Client;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11ScenarioTests;

[TestClass]
public sealed class RemovableSlotTest
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
                IsRemovableDevice = true,
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
    public void Slot_IsRemovableDevice_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BchClient.P11LibPath,
            AppType.SingleThreaded);

        Assert.IsTrue(SlotId.HasValue);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithOrWithoutTokenPresent);
        ISlot slot = slots.Where(t => t.SlotId == (ulong)(SlotId.Value)).Single();

        ISlotInfo slotInfo = slot.GetSlotInfo();

        Assert.IsTrue(slotInfo.SlotFlags.RemovableDevice);
    }


    [TestMethod]
    public async Task Slot_PluggedUnpluged_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BchClient.P11LibPath,
            AppType.SingleThreaded);

        Assert.IsTrue(SlotId.HasValue);

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = true
        });

        {
            List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
            int slotCount = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Count();

            Assert.AreEqual(1, slotCount);
        }

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = false
        });

        {
            List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
            int slotCount = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Count();

            Assert.AreEqual(0, slotCount);
        }
    }

    [TestMethod]
    public async Task Slot_UnpluggedPluged_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BchClient.P11LibPath,
            AppType.SingleThreaded);

        Assert.IsTrue(SlotId.HasValue);

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = false
        });

        {
            List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
            int slotCount = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Count();

            Assert.AreEqual(0, slotCount);
        }

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = true
        });

        {
            List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
            int slotCount = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Count();

            Assert.AreEqual(1, slotCount);
        }
    }

    [TestMethod]
    public async Task WaitForSlotEvent_PluggedUnpluged_Success()
    {
        Assert.IsTrue(SlotId.HasValue);

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = true
        });

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BchClient.P11LibPath,
            AppType.SingleThreaded);

        
        bool eventOccured;
        ulong slotId;

        library.WaitForSlotEvent(WaitType.NonBlocking, out eventOccured, out slotId);
        Assert.IsFalse(eventOccured);

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = false
        });

        library.WaitForSlotEvent(WaitType.NonBlocking, out eventOccured, out slotId);
        Assert.IsTrue(eventOccured);
        Assert.AreEqual((ulong)SlotId.Value, slotId);

        library.WaitForSlotEvent(WaitType.NonBlocking, out eventOccured, out slotId);
        Assert.IsFalse(eventOccured);

        await BchClient.Client.SetSlotPluggedStateAsync(SlotId.Value, new SetPluggedStateDto()
        {
            Plugged = true
        });

        library.WaitForSlotEvent(WaitType.NonBlocking, out eventOccured, out slotId);
        Assert.IsTrue(eventOccured);
        Assert.AreEqual((ulong)SlotId.Value, slotId);
    }
}