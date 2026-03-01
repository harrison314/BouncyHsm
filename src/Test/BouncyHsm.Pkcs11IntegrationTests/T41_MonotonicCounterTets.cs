using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T41_MonotonicCounterTets
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void GetAttributeValue_HwMonotonicCounter_Success()
    {
        const int tryCount = 3;

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        HashSet<string> counetrValues = new HashSet<string>();
        for (int i = 0; i < tryCount; i++)
        {
            List<IObjectHandle> handles = session.FindAllObjects(new List<IObjectAttribute>()
            {
                factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_HW_FEATURE),
                factories.ObjectAttributeFactory.Create(CKA.CKA_HW_FEATURE_TYPE,(uint)CKH.CKH_MONOTONIC_COUNTER)
            });

            IObjectHandle clockObject = handles.Single();

            List<IObjectAttribute> values = session.GetAttributeValue(clockObject, new List<CKA>()
            {
                CKA.CKA_VALUE
            });

            byte[] counterValue = values.Single().GetValueAsByteArray();
            string counterValueHex = Convert.ToHexString(counterValue);

            this.TestContext?.WriteLine("Counter value {0} - {1}", i, counterValueHex);
            Assert.IsTrue(counetrValues.Add(counterValueHex), "New Counter value is not uniq.");
        }
    }
}
