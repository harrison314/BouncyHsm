using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T14_GenerateRandom
{
    [TestMethod]
    public void GenerateRandom_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        byte[] data = session.GenerateRandom(14);

        Assert.IsNotNull(data);
        Assert.AreEqual(14, data.Length);
    }

    [TestMethod]
    public void GenerateRandom_CallRepeat_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        byte[] data = session.GenerateRandom(32);
        byte[] data2 = session.GenerateRandom(32);
        byte[] data3 = session.GenerateRandom(32);
        byte[] data4 = session.GenerateRandom(32);

        Assert.IsFalse(data.SequenceEqual(data2));
        Assert.IsFalse(data.SequenceEqual(data3));
        Assert.IsFalse(data.SequenceEqual(data4));
        Assert.IsFalse(data2.SequenceEqual(data3));
        Assert.IsFalse(data2.SequenceEqual(data4));
        Assert.IsFalse(data3.SequenceEqual(data4));
    }

    [TestMethod]
    public void GenerateRandom_CallRepeatWithSession_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        Func<ISlot, byte[]> getRandomFn = slot =>
        {
            using ISession session = slot.OpenSession(SessionType.ReadOnly);
            return session.GenerateRandom(32);
        };
        
        byte[] data = getRandomFn(slot);
        byte[] data2 = getRandomFn(slot);
        byte[] data3 = getRandomFn(slot);
        byte[] data4 = getRandomFn(slot);

        Assert.IsFalse(data.SequenceEqual(data2));
        Assert.IsFalse(data.SequenceEqual(data3));
        Assert.IsFalse(data.SequenceEqual(data4));
        Assert.IsFalse(data2.SequenceEqual(data3));
        Assert.IsFalse(data2.SequenceEqual(data4));
        Assert.IsFalse(data3.SequenceEqual(data4));
    }
}
