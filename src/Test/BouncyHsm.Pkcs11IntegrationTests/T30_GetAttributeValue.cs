using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T30_GetAttributeValue
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void GetAttributeValue_SecretKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"Seecret-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        byte[] secret = new byte[32];
        Random.Shared.NextBytes(secret);

        List<IObjectAttribute> objectAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_SHA256_HMAC),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_MODIFIABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_COPYABLE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, secret),
        };

        IObjectHandle key = session.CreateObject(objectAttributes);

        List<CKA> getTemplate = new List<CKA>()
        {
            CKA.CKA_KEY_TYPE,
            CKA.CKA_EXTRACTABLE,
            CKA.CKA_COPYABLE,
            CKA.CKA_LABEL,
            CKA.CKA_VALUE_LEN,
            CKA.CKA_VALUE
        };

        List<IObjectAttribute> attributes = session.GetAttributeValue(key, getTemplate);

        Assert.AreEqual(CKK.CKK_SHA256_HMAC, (CKK) attributes[0].GetValueAsUlong());
        Assert.AreEqual(true, attributes[1].GetValueAsBool());
        Assert.AreEqual(false, attributes[2].GetValueAsBool());
        Assert.AreEqual(label, attributes[3].GetValueAsString());
        Assert.AreEqual((ulong)secret.Length, attributes[4].GetValueAsUlong());
        Assert.IsTrue(secret.SequenceEqual(attributes[5].GetValueAsByteArray()));
    }

    //[TestMethod]
    //public void GetAttributeValue_SensitiveValue_Success()
    //{
    //    throw new NotImplementedException();
    //}
}
