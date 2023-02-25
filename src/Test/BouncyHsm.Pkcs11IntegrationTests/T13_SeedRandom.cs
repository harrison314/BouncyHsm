using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T13_SeedRandom
{
    [TestMethod]
    public void GenerateRandom_Call_Success()
    {
        byte[] seedData = new byte[64];
        Random.Shared.NextBytes(seedData);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        try
        {
            session.SeedRandom(seedData);
        }
        catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_RANDOM_SEED_NOT_SUPPORTED)
        {
            // not supported is valid return code
        }
    }
}
