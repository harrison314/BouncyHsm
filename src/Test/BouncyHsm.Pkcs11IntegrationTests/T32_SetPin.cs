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
public class T32_SetPin
{
    private const string OtherUserPin = "zuj>UxK^Zy1/+*WsGfb!6+";
    private const string OtherSoPin = "gXnka_,{AM}D.'\"3/YmH2x";

    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void SetPin_NotLogged_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            session.SetPin(AssemblyTestConstants.UserPin, OtherUserPin);
        }

        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            session.Login(CKU.CKU_USER, OtherUserPin);
            session.SetPin(OtherUserPin, AssemblyTestConstants.UserPin);
        }
    }

    [TestMethod]
    public void SetPin_LoggedUser_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);
            session.SetPin(AssemblyTestConstants.UserPin, OtherUserPin);
        }

        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            session.Login(CKU.CKU_USER, OtherUserPin);
            session.SetPin(OtherUserPin, AssemblyTestConstants.UserPin);
        }
    }

    [TestMethod]
    public void SetPin_LoggedSo_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            session.Login(CKU.CKU_SO, AssemblyTestConstants.SoPin);
            session.SetPin(AssemblyTestConstants.SoPin, OtherSoPin);
        }

        using (ISession session = slot.OpenSession(SessionType.ReadWrite))
        {
            session.Login(CKU.CKU_SO, OtherSoPin);
            session.SetPin(OtherSoPin, AssemblyTestConstants.SoPin);
        }
    }

    [TestMethod]
    public void SetPin_BadPin_Error()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        Assert.ThrowsException<Pkcs11Exception>(() => session.SetPin("bad pin...", OtherUserPin));
    }

    [TestMethod]
    public void SetPin_BadPinLogged_Error()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        Assert.ThrowsException<Pkcs11Exception>(() => session.SetPin("bad pin...", OtherUserPin));
    }
}
