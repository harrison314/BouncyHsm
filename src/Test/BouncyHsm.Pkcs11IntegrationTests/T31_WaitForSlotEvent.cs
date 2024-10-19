using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T31_WaitForSlotEvent
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void WaitForSlotEvent_NotBlock_ReturnsNoEvent()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        library.WaitForSlotEvent(WaitType.NonBlocking, out bool eventOccured, out ulong slotId);

        Assert.IsFalse(eventOccured, $"Invalid return CKR. eventOccured - {eventOccured}, slotId {slotId}");
    }
}
