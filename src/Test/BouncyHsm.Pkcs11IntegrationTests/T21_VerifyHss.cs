using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.Common;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T21_VerifyHss
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Verify_Valid_And_Tampered()
    {
        this.VerifyValidAndTampered(1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 });
    }

    [TestMethod]
    public void Verify_TwoLevels_Valid_And_Tampered()
    {
        this.VerifyValidAndTampered(2,
            new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5, CK_LMS_TYPE.LMS_SHA256_M32_H5 },
            new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8, CK_LMOTS_TYPE.LMOTS_SHA256_N32_W1 });
    }

    [TestMethod]
    public void VerifyMultiPart_FunctionNotSupported()
    {
        byte[] dataToSign = RandomNumberGenerator.GetBytes(64);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = T20_SignHss.LoadLibrary(factories);
        using ISession session = T20_SignHss.OpenLoggedSession(library);

        T20_SignHss.CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, false, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        using MemoryStream input = new MemoryStream(dataToSign);
        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.Verify(mechanism, publicKey, input, signature, out bool _));
        Assert.AreEqual(CKR.CKR_FUNCTION_NOT_SUPPORTED, ex.RV);

        // The failed multi-part operation must not leave the verify operation active.
        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");
    }

    private void VerifyValidAndTampered(ulong levels, ulong[] lmsTypes, ulong[] lmotsTypes)
    {
        byte[] dataToSign = RandomNumberGenerator.GetBytes(64);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = T20_SignHss.LoadLibrary(factories);
        using ISession session = T20_SignHss.OpenLoggedSession(library);

        T20_SignHss.CreateHssKeyPair(factories, session, levels, lmsTypes, lmotsTypes, false, out IObjectHandle publicKey, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        dataToSign[3] ^= 0xFF;
        session.Verify(mechanism, publicKey, dataToSign, signature, out isValid);
        Assert.IsFalse(isValid, "Signature is valid.");
    }
}
