using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue160
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    [DataRow(CKM.CKM_EXTRACT_KEY_FROM_KEY)]
    [DataRow(CKM.CKM_XOR_BASE_AND_DATA)]
    [DataRow(CKM.CKM_CONCATENATE_BASE_AND_DATA)]
    [DataRow(CKM.CKM_CONCATENATE_DATA_AND_BASE)]
    public void DeriveKey_ProtectionInheritedFromBaseKey(CKM mechanismType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        byte[] secret = Convert.FromHexString("3C9107E45AD2681FB34E961FC77D02AF");
        IObjectHandle baseKey = session.CreateObject(new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        });

        byte[] mechanismData = mechanismType == CKM.CKM_XOR_BASE_AND_DATA
            ? new byte[secret.Length]
            : Convert.FromHexString("DEADBEEF");
        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkExtractParams extractParams =
            factories.MechanismParamsFactory.CreateCkExtractParams(0U);
        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkKeyDerivationStringData dataParams =
            factories.MechanismParamsFactory.CreateCkKeyDerivationStringData(mechanismData);

        using IMechanism mechanism = mechanismType == CKM.CKM_EXTRACT_KEY_FROM_KEY
            ? factories.MechanismFactory.Create(mechanismType, extractParams)
            : factories.MechanismFactory.Create(mechanismType, dataParams);

        uint derivedKeyLength = mechanismType is CKM.CKM_CONCATENATE_BASE_AND_DATA or CKM.CKM_CONCATENATE_DATA_AND_BASE
            ? (uint)(secret.Length + mechanismData.Length)
            : (uint)secret.Length;
        IObjectHandle derivedKey = session.DeriveKey(mechanism, baseKey, new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, derivedKeyLength),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
        });

        List<IObjectAttribute> protectionAttributes = session.GetAttributeValue(derivedKey, new List<CKA>()
        {
            CKA.CKA_SENSITIVE,
            CKA.CKA_EXTRACTABLE,
            CKA.CKA_ALWAYS_SENSITIVE,
            CKA.CKA_NEVER_EXTRACTABLE,
        });
        Assert.IsTrue(protectionAttributes[0].GetValueAsBool(), $"{mechanismType}: derived key must be sensitive.");
        Assert.IsFalse(protectionAttributes[1].GetValueAsBool(), $"{mechanismType}: derived key must not be extractable.");
        Assert.IsTrue(protectionAttributes[2].GetValueAsBool(), $"{mechanismType}: derived key must always be sensitive.");
        Assert.IsTrue(protectionAttributes[3].GetValueAsBool(), $"{mechanismType}: derived key must never be extractable.");

        IObjectAttribute valueAttribute = session.GetAttributeValue(derivedKey, new List<CKA>() { CKA.CKA_VALUE })[0];
        Assert.IsTrue(valueAttribute.CannotBeRead, $"{mechanismType}: derived key value must be protected.");

        session.DestroyObject(derivedKey);
        session.DestroyObject(baseKey);
    }
}
