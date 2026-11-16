using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue147
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKM_V3_0.CKM_SHA512_224_KEY_GEN, CKM.CKM_SHA512_224_HMAC)]
    [DataRow(CKM_V3_0.CKM_SHA512_256_KEY_GEN, CKM.CKM_SHA512_256_HMAC)]
    public void SignHmac_Data_Success(CKM generateMechanism, CKM signatureMechanism)
    {
        byte[] data = Utils.GetRandomBytes(32);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle keyHandle = this.GenerateKey(session, generateMechanism);

        IMechanism mechanism = factories.MechanismFactory.Create(signatureMechanism);
        byte[] signature = session.Sign(mechanism, keyHandle, data);

        Assert.IsNotNull(signature);
    }

    private IObjectHandle GenerateKey(ISession session, CKM generateMechanism)
    {

        Pkcs11InteropFactories factories = session.Factories;

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 28U),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
        };

        using IMechanism mechanism = factories.MechanismFactory.Create(generateMechanism);

        return session.GenerateKey(mechanism, keyAttributes);
    }
}
