using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignPoly1305
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, CKM_V3_0.CKM_POLY1305)]
    [DataRow(CKK_V3_0.CKK_POLY1305, CKM_V3_0.CKM_POLY1305)]
    public void Sign_Poly1305_Success(CKK type, CKM signatureMechanism)
    {
        byte[] dataToSign = new byte[64];
        Random.Shared.NextBytes(dataToSign);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"PpolySeecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);
        this.GenerateSeecret(type, 32, factories, session, label, ckId);

        IObjectHandle handle = this.FindSeecretKey(session, ckId, label);

        using IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);

        byte[] signature = session.Sign(mechanism, handle, dataToSign);
        byte[] seecrit = this.GetSeecretKeyValue(session, handle);

        session.DestroyObject(handle);
    }

    private void GenerateSeecret(CKK type, int size, Pkcs11InteropFactories factories, ISession session, string label, byte[] ckId)
    {
        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, type),

            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        if (type == CKK_V3_0.CKK_POLY1305)
        {
            using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_0.CKM_POLY1305_KEY_GEN);
            _ = session.GenerateKey(mechanism, keyAttributes);
        }
        else
        {
            using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_GENERIC_SECRET_KEY_GEN);
            _ = session.GenerateKey(mechanism, keyAttributes);
        }
    }

    private IObjectHandle FindSeecretKey(ISession session, byte[] ckaId, string ckaLabel)
    {
        List<IObjectAttribute> searchTemplate = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckaId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, ckaLabel)
        };

        return session.FindAllObjects(searchTemplate).Single();
    }

    private byte[] GetSeecretKeyValue(ISession session, IObjectHandle handle)
    {
        return session.GetAttributeValue(handle, new List<CKA>() { CKA.CKA_VALUE })
            .Single()
            .GetValueAsByteArray();
    }
}