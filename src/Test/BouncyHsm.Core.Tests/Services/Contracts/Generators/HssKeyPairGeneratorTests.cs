using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;

namespace BouncyHsm.Core.Tests.Services.Contracts.Generators;

[TestClass]
public class HssKeyPairGeneratorTests
{
    private static HssKeyPairGenerator Create() => new HssKeyPairGenerator(new NullLogger<HssKeyPairGenerator>());

    private static Dictionary<CKA, IAttributeValue> PrivTemplate(uint levels, uint[] lms, uint[] ots)
    {
        return new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_HSS_LEVELS, AttributeValue.Create(levels) },
            {CKA.CKA_HSS_LMS_TYPES, AttributeValue.Create(lms) },
            {CKA.CKA_HSS_LMOTS_TYPES, AttributeValue.Create(ots) },
            {CKA.CKA_SIGN, AttributeValue.Create(true) }
        };
    }

    private static Dictionary<CKA, IAttributeValue> PubTemplate()
    {
        return new Dictionary<CKA, IAttributeValue>() { { CKA.CKA_VERIFY, AttributeValue.Create(true) } };
    }

    [TestMethod]
    public void Generate_OneLevel_Success()
    {
        HssKeyPairGenerator generator = Create();
        generator.Init(PubTemplate(), PrivTemplate(1, new uint[] { 5 }, new uint[] { 4 }));

        (PublicKeyObject pub, PrivateKeyObject priv) = generator.Generate(new SecureRandom());

        HssPrivateKeyObject hssPriv = (HssPrivateKeyObject)priv;
        HssPublicKeyObject hssPub = (HssPublicKeyObject)pub;
        Assert.AreEqual(32U, hssPriv.CkaHssKeysRemaining);
        Assert.AreEqual(1U, hssPub.CkaHssLevels);
        Assert.AreEqual(CK_LMS_TYPE.LMS_SHA256_M32_H5, hssPub.CkaHssLmsType);
        Assert.AreEqual(60, hssPub.CkaValue.Length);
        Assert.IsTrue(hssPriv.CkaSign);
        Assert.IsTrue(hssPub.CkaVerify);

        Assert.IsTrue(hssPriv.CkaSensitive);
        Assert.IsFalse(hssPriv.CkaExtractable);
        Assert.IsFalse(hssPriv.CkaCopyable);

        hssPub.ReComputeAttributes();
        hssPriv.ReComputeAttributes();
        hssPub.Validate();
        hssPriv.Validate();
    }

    [TestMethod]
    public void Generate_TwoLevels_KeysRemaining1024()
    {
        HssKeyPairGenerator generator = Create();
        generator.Init(PubTemplate(), PrivTemplate(2, new uint[] { 5, 5 }, new uint[] { 4, 4 }));

        (PublicKeyObject _, PrivateKeyObject priv) = generator.Generate(new SecureRandom());

        Assert.AreEqual(1024U, ((HssPrivateKeyObject)priv).CkaHssKeysRemaining);
    }

    [TestMethod]
    public void Init_MissingLevels_ThrowsTemplateIncomplete()
    {
        Dictionary<CKA, IAttributeValue> priv = PrivTemplate(1, new uint[] { 5 }, new uint[] { 4 });
        priv.Remove(CKA.CKA_HSS_LEVELS);

        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => Create().Init(PubTemplate(), priv));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCOMPLETE, ex.ReturnValue);
    }

    [TestMethod]
    public void Init_CountMismatch_ThrowsTemplateInconsistent()
    {
        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(
            () => Create().Init(PubTemplate(), PrivTemplate(2, new uint[] { 5 }, new uint[] { 4, 4 })));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    public void Init_ExtractableTrue_ThrowsTemplateInconsistent()
    {
        Dictionary<CKA, IAttributeValue> priv = PrivTemplate(1, new uint[] { 5 }, new uint[] { 4 });
        priv[CKA.CKA_EXTRACTABLE] = AttributeValue.Create(true);

        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => Create().Init(PubTemplate(), priv));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    public void Init_SensitiveFalse_ThrowsTemplateInconsistent()
    {
        Dictionary<CKA, IAttributeValue> priv = PrivTemplate(1, new uint[] { 5 }, new uint[] { 4 });
        priv[CKA.CKA_SENSITIVE] = AttributeValue.Create(false);

        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => Create().Init(PubTemplate(), priv));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    public void Init_CopyableTrue_ThrowsTemplateInconsistent()
    {
        Dictionary<CKA, IAttributeValue> priv = PrivTemplate(1, new uint[] { 5 }, new uint[] { 4 });
        priv[CKA.CKA_COPYABLE] = AttributeValue.Create(true);

        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => Create().Init(PubTemplate(), priv));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCONSISTENT, ex.ReturnValue);
    }

    [TestMethod]
    [DataRow(CKA.CKA_HSS_LMS_TYPES)]
    [DataRow(CKA.CKA_HSS_LMOTS_TYPES)]
    public void Init_MissingTypeArray_ThrowsTemplateIncomplete(CKA missing)
    {
        Dictionary<CKA, IAttributeValue> priv = PrivTemplate(1, new uint[] { 5 }, new uint[] { 4 });
        priv.Remove(missing);

        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(() => Create().Init(PubTemplate(), priv));
        Assert.AreEqual(CKR.CKR_TEMPLATE_INCOMPLETE, ex.ReturnValue);
    }

    [TestMethod]
    public void MechanismUtils_HssRegistered()
    {
        Assert.IsTrue(MechanismUtils.TryGetMechanismInfo((uint)CKM.CKM_HSS, out MechanismInfo info));
        Assert.AreEqual(0, (int)info.MinKeySize);
        Assert.AreEqual(0, (int)info.MaxKeySize);
        Assert.IsTrue(info.Flags.HasFlag(MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY));
        Assert.IsTrue(MechanismUtils.TryGetMechanismInfo((uint)CKM.CKM_HSS_KEY_PAIR_GEN, out _));
    }
}
