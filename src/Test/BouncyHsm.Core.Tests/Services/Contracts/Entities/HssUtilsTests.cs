using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.Services.Contracts.Entities;

[TestClass]
public class HssUtilsTests
{
    [TestMethod]
    public void GetLmsParameters_AllEnumValues_RoundTrip()
    {
        foreach (CK_LMS_TYPE v in Enum.GetValues<CK_LMS_TYPE>())
        {
            var p = HssUtils.GetLmsParameters(v);
            Assert.AreEqual((int)v, p.ID);
            Assert.AreEqual(v, HssUtils.GetLmsType(p));
        }
    }

    [TestMethod]
    public void GetLmOtsParameters_AllEnumValues_RoundTrip()
    {
        foreach (CK_LMOTS_TYPE v in Enum.GetValues<CK_LMOTS_TYPE>())
        {
            var p = HssUtils.GetLmOtsParameters(v);
            Assert.AreEqual((int)v, p.ID);
            Assert.AreEqual(v, HssUtils.GetLmOtsType(p));
        }
    }

    [TestMethod]
    public void CreateParameters_CountMismatch_ThrowsTemplateInconsistent()
    {
        var ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => HssUtils.CreateParameters(2, new uint[] { 5 }, new uint[] { 4, 4 }));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    [DataRow(0U)]
    [DataRow(9U)]
    public void CreateParameters_LevelsOutOfRange_Throws(uint levels)
    {
        var ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => HssUtils.CreateParameters(levels, new uint[levels], new uint[levels]));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    public void CreateParameters_UnknownCode_Throws()
    {
        var ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => HssUtils.CreateParameters(1, new uint[] { 0x19 }, new uint[] { 4 }));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    [DataRow(new uint[] { 0x0a, 0x05 }, new uint[] { 0x05, 0x04 })]
    [DataRow(new uint[] { 0x05, 0x05 }, new uint[] { 0x05, 0x04 })]
    [DataRow(new uint[] { 0x05, 0x14, 0x0f }, new uint[] { 0x02, 0x10, 0x03 })]
    public void CreateParameters_ChildHashLargerThanParentOts_Throws(uint[] lmsTypes, uint[] lmotsTypes)
    {
        var ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => HssUtils.CreateParameters((uint)lmsTypes.Length, lmsTypes, lmotsTypes));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    [DataRow(new uint[] { 0x05, 0x0a }, new uint[] { 0x05, 0x04 })]
    [DataRow(new uint[] { 0x0a, 0x05 }, new uint[] { 0x04, 0x04 })]
    [DataRow(new uint[] { 0x05, 0x0f }, new uint[] { 0x04, 0x0c })]
    [DataRow(new uint[] { 0x14, 0x14 }, new uint[] { 0x0d, 0x0d })]
    public void CreateParameters_SupportedMixedLevels_Generate(uint[] lmsTypes, uint[] lmotsTypes)
    {
        Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator generator = new Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator();
        generator.Init(new Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyGenerationParameters(
            HssUtils.CreateParameters((uint)lmsTypes.Length, lmsTypes, lmotsTypes),
            new Org.BouncyCastle.Security.SecureRandom()));

        Assert.IsNotNull(generator.GenerateKeyPair().Private);
    }

    [TestMethod]
    public void CreateParameters_Valid_ReturnsLevels()
    {
        var result = HssUtils.CreateParameters(2, new uint[] { 5, 5 }, new uint[] { 4, 4 });
        Assert.AreEqual(2, result.Length);
    }

    [TestMethod]
    public void ToKeysRemaining_Clamps()
    {
        Assert.AreEqual(uint.MaxValue, HssUtils.ToKeysRemaining(1L << 40));
        Assert.AreEqual(31U, HssUtils.ToKeysRemaining(31));
        Assert.AreEqual(0U, HssUtils.ToKeysRemaining(-1));
    }

    [TestMethod]
    public void GetParametersName_Format()
    {
        Assert.AreEqual("HSS L=2 LMS_SHA256_M32_H5/LMOTS_SHA256_N32_W8",
            HssUtils.GetParametersName(CK_LMS_TYPE.LMS_SHA256_M32_H5, CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8, 2));
    }

    [TestMethod]
    [DataRow(new uint[] { 0x05 }, new uint[] { 0x04 }, 1296U)]
    [DataRow(new uint[] { 0x05, 0x05 }, new uint[] { 0x04, 0x04 }, 2644U)]
    [DataRow(new uint[] { 0x14 }, new uint[] { 0x0d })]
    [DataRow(new uint[] { 0x0f, 0x0a }, new uint[] { 0x0a, 0x07 })]
    [DataRow(new uint[] { 0x05, 0x14, 0x0a }, new uint[] { 0x02, 0x10, 0x03 })]
    public void GetSignatureLength_MatchesRealSignature(uint[] lmsTypes, uint[] lmotsTypes, uint expected = 0)
    {
        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(HssUtils.CreateParameters((uint)lmsTypes.Length, lmsTypes, lmotsTypes), new SecureRandom()));
        HssPrivateKeyParameters privateKey = (HssPrivateKeyParameters)generator.GenerateKeyPair().Private;

        HssSigner signer = new HssSigner();
        signer.Init(true, privateKey);
        byte[] signature = signer.GenerateSignature(new byte[] { 1, 2, 3 });

        uint computed = HssUtils.GetSignatureLength(lmsTypes, lmotsTypes);
        Assert.AreEqual((uint)signature.Length, computed);
        if (expected != 0)
        {
            Assert.AreEqual(expected, computed);
        }
    }
}
