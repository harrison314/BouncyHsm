using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T37_SessionCancel
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void SessionCancel_WithoutFlagWithoutOperation_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        session.SessionCancel(library, 0);
    }

    [TestMethod]
    [DataRow(CKF.CKF_DECRYPT)]
    [DataRow(CKF.CKF_DIGEST)]
    [DataRow(CKF.CKF_ENCRYPT)]
    [DataRow(CKF.CKF_SIGN)]
    [DataRow(CKF.CKF_SIGN_RECOVER)]
    [DataRow(CKF.CKF_VERIFY)]
    [DataRow(CKF.CKF_VERIFY_RECOVER)]
    [DataRow(CKF.CKF_VERIFY | CKF.CKF_SIGN | CKF.CKF_DECRYPT)]
    [DataRow(CKF.CKF_DIGEST | CKF.CKF_SIGN)]
    public void SessionCancel_WithFlagWithoutOperation_Success(uint ckfFlag)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        session.SessionCancel(library, ckfFlag);
    }

    [TestMethod]
    public void SessionCancel_CancelFind_Success()
    {
        const uint CKF_FIND_OBJECTS = 0x00000040;

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        session.FindObjectsInit(new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
        });

        session.SessionCancel(library, CKF_FIND_OBJECTS);

        session.FindObjectsInit(new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
        });
    }
}
