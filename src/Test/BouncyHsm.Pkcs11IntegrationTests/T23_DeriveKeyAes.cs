using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Data;
using System.Drawing;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T23_DeriveKeyAes
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Derive_AesEcb_Success()
    {
        byte[] data = new byte[64];
        Random.Shared.NextBytes(data);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle handle = this.GenerateAesKey(session);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> newKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true)
        };

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData mechanismParam = factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(data);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_AES_ECB_ENCRYPT_DATA, mechanismParam);
        IObjectHandle derivedHandle = session.DeriveKey(mechanism, handle, newKeyAttributes);
    }

    private IObjectHandle GenerateAesKey(ISession session)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)32),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}