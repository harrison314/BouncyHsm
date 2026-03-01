using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Pkcs11ScenarioTests;

[TestClass]
public sealed class MonotonicCounterTest
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
            Client.CreateSlotResultDto information = await BchClient.Client.CreateSlotAsync(new Client.CreateSlotDto()
            {
                Description = $"Integration Test Slot - {runId}",
                IsHwDevice = true,
                IsRemovableDevice = false,
                Token = new Client.CreateTokenDto()
                {
                    Label = $"IntegrationTestSlot-{runId}",
                    SerialNumber = null,
                    SimulateHwMechanism = true,
                    SimulateHwRng = false,
                    SimulateProtectedAuthPath = false,
                    SimulateQualifiedArea = false,
                    SpeedMode = Client.SpeedMode.WithoutRestriction,
                    SignaturePin = null,
                    SoPin = "12345678",
                    UserPin = LoginPin,
                }
            });

            SlotId = information.SlotId;
            TokenSerialNumber = information.TokenSerialNumber;
        }
        catch (Client.ApiBouncyHsmException<Client.ProblemDetails> ex)
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
    public void SwRng_RngFlag_IsFalse()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            BchClient.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.Where(t => t.GetTokenInfo().SerialNumber == TokenSerialNumber).Single();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, LoginPin);

        List<IObjectHandle> handles = session.FindAllObjects(new List<IObjectAttribute>()
            {
                factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_HW_FEATURE),
                factories.ObjectAttributeFactory.Create(CKA.CKA_HW_FEATURE_TYPE,(uint)CKH.CKH_MONOTONIC_COUNTER)
            });

        IObjectHandle clockObject = handles.Single();

        List<IObjectAttribute> values1 = session.GetAttributeValue(clockObject, new List<CKA>()
        {
            CKA.CKA_VALUE,
            CKA.CKA_HAS_RESET
        });

        string value1 = Convert.ToHexString(values1[0].GetValueAsByteArray());
        Assert.IsFalse(values1[1].GetValueAsBool(), "CKA_HAS_RESET invalid value (except false).");

        slot.InitToken("12345678", "Integration Test Token");

        List<IObjectAttribute> values2 = session.GetAttributeValue(clockObject, new List<CKA>()
        {
            CKA.CKA_VALUE,
            CKA.CKA_HAS_RESET
        });

        string value2 = Convert.ToHexString(values2[0].GetValueAsByteArray());
        Assert.IsTrue(values2[1].GetValueAsBool(), "CKA_HAS_RESET invalid value (except true).");

        Assert.AreEqual(value1, value2, "Monotonic counter is not reset.");
    }
}
