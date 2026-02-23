using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T40_GetSessionValidationFlags
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void GetSessionValidationFlags_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IMechanism digestMech = factories.MechanismFactory.Create(CKM.CKM_SHA256);
        _ = session.Digest(digestMech, new byte[] { 1, 2, 3, 5, 6, 8, 7, 8, 9 });

        ulong flags = session.GetSessionValidationFlags(library, CK_SESSION_VALIDATION_FLAGS.CKS_LAST_VALIDATION_OK);
    }
}
