using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation.Visitors;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.Services.Contracts.Entities;

[TestClass]
public class HssKeyObjectTests
{
    private static Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair GenerateKeyPair()
    {
        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(
            new[] { new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8) },
            new SecureRandom()));
        return generator.GenerateKeyPair();
    }

    private static HssPrivateKeyObject CreatePrivate(out HssPublicKeyParameters publicKey)
    {
        var pair = GenerateKeyPair();
        publicKey = (HssPublicKeyParameters)pair.Public;
        HssPrivateKeyObject obj = new HssPrivateKeyObject();
        obj.SetPrivateKey(pair.Private);
        return obj;
    }

    private static void AssertRpc(CKR expected, Action action)
    {
        RpcPkcs11Exception ex = Assert.ThrowsExactly<RpcPkcs11Exception>(action);
        Assert.AreEqual(expected, ex.ReturnValue);
    }

    [TestMethod]
    public void PrivateKey_Defaults_SensitiveNotExtractableNotCopyable()
    {
        HssPrivateKeyObject obj = new HssPrivateKeyObject();

        Assert.IsTrue(obj.CkaSensitive);
        Assert.IsFalse(obj.CkaExtractable);
        Assert.IsFalse(obj.CkaCopyable);
    }

    [TestMethod]
    public void PrivateKey_SetPrivateKey_FillsAttributes()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);

        Assert.AreEqual(1u, obj.CkaHssLevels);
        CollectionAssert.AreEqual(new uint[] { 5 }, obj.CkaHssLmsTypes);
        CollectionAssert.AreEqual(new uint[] { 4 }, obj.CkaHssLmotsTypes);
        Assert.AreEqual(32u, obj.CkaHssKeysRemaining);
        obj.Validate();
    }

    [TestMethod]
    public void PrivateKey_KeysRemaining_IsReadOnly()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);

        AssertRpc(CKR.CKR_ATTRIBUTE_READ_ONLY,
            () => obj.SetValue(CKA.CKA_HSS_KEYS_REMAINING, AttributeValue.Create(5u), true));
    }

    [TestMethod]
    public void PrivateKey_Validate_ExtractableTrue_Throws()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);
        obj.CkaExtractable = true;
        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, obj.Validate);
    }

    [TestMethod]
    public void PrivateKey_Validate_CopyableTrue_Throws()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);
        obj.CkaCopyable = true;
        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, obj.Validate);
    }

    [TestMethod]
    public void PrivateKey_Validate_SensitiveFalse_Throws()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);
        obj.CkaSensitive = false;
        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, obj.Validate);
    }

    [TestMethod]
    public void PrivateKey_Validate_TypeMismatch_Throws()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);
        obj.CkaHssLmsTypes = new uint[] { (uint)CK_LMS_TYPE.LMS_SHA256_M32_H10 };
        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, obj.Validate);
    }

    [TestMethod]
    public void PrivateKey_MementoRoundTrip_KeepsIndex()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);

        obj.Id = Guid.NewGuid();
        HssPrivateKeyParameters priv = (HssPrivateKeyParameters)obj.GetPrivateKey();
        HssSigner signer = new HssSigner();
        signer.Init(true, priv);
        signer.GenerateSignature(new byte[] { 1, 2, 3 });
        obj.SetPrivateKey(priv);

        HssPrivateKeyObject restored = (HssPrivateKeyObject)StorageObjectFactory.CreateFromMemento(obj.ToMemento());

        Assert.AreEqual(31u, restored.CkaHssKeysRemaining);
        Assert.AreEqual(31L, ((HssPrivateKeyParameters)restored.GetPrivateKey()).GetUsagesRemaining());
    }

    [TestMethod]
    public void PublicKey_Validate_TypeMismatch_Throws()
    {
        var pair = GenerateKeyPair();
        HssPublicKeyObject obj = new HssPublicKeyObject();
        obj.SetPublicKey(pair.Public);
        obj.Validate();

        obj.CkaHssLmsType = CK_LMS_TYPE.LMS_SHA256_M32_H10;
        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, obj.Validate);
    }

    [TestMethod]
    public void PublicKey_ReComputeAttributes_DerivesFromValue()
    {
        var pair = GenerateKeyPair();
        HssPublicKeyObject obj = new HssPublicKeyObject();
        obj.CkaValue = ((HssPublicKeyParameters)pair.Public).GetEncoded();

        obj.ReComputeAttributes();
        obj.Validate();

        Assert.AreEqual(1u, obj.CkaHssLevels);
        Assert.AreEqual(CK_LMS_TYPE.LMS_SHA256_M32_H5, obj.CkaHssLmsType);
        Assert.AreEqual(CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8, obj.CkaHssLmotsType);
    }

    [TestMethod]
    public void PrivateKey_GetValue_CkaValue_IsSensitive()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);

        AttributeValueResult result = obj.GetValue(CKA.CKA_VALUE, CryptoApiObjectGetValueMode.Default);

        Assert.IsInstanceOfType<AttributeValueResult.SensitiveOrUnextractable>(result);
    }

    [TestMethod]
    public void PrivateKey_CreateObjectFlow_DerivesKeysRemaining()
    {
        HssPrivateKeyObject source = CreatePrivate(out _);
        HssPrivateKeyParameters priv = (HssPrivateKeyParameters)source.GetPrivateKey();
        HssSigner signer = new HssSigner();
        signer.Init(true, priv);
        signer.GenerateSignature(new byte[] { 1 });

        HssPrivateKeyObject obj = new HssPrivateKeyObject();
        obj.SetValue(CKA.CKA_HSS_LEVELS, AttributeValue.Create(1u), false);
        obj.SetValue(CKA.CKA_HSS_LMS_TYPES, AttributeValue.Create(new uint[] { 5 }), false);
        obj.SetValue(CKA.CKA_HSS_LMOTS_TYPES, AttributeValue.Create(new uint[] { 4 }), false);
        obj.SetValue(CKA.CKA_VALUE, AttributeValue.Create(priv.GetEncoded()), false);

        obj.ReComputeAttributes();
        obj.Validate();

        Assert.AreEqual(31u, obj.CkaHssKeysRemaining);
    }

    [TestMethod]
    public void ObjectContentVisitor_PublicKey_ReturnsPem()
    {
        HssPublicKeyObject obj = new HssPublicKeyObject();
        obj.SetPublicKey(GenerateKeyPair().Public);

        var result = obj.Accept(new ObjectContentVisitor());

        var ok = Assert.IsInstanceOfType<DomainResult<ObjectContent>.Ok>(result);
        Assert.AreEqual("hss_public_key.pem", ok.Value.FileName);
        StringAssert.StartsWith(System.Text.Encoding.ASCII.GetString(ok.Value.Content), "-----BEGIN PUBLIC KEY-----");
    }

    [TestMethod]
    public void ObjectContentVisitor_PrivateKey_IsInvalidInput()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);

        var result = obj.Accept(new ObjectContentVisitor());

        Assert.IsInstanceOfType<DomainResult<ObjectContent>.InvalidInput>(result);
    }

    [TestMethod]
    public void Factory_CreateEmpty_HssTemplates()
    {
        var priv = StorageObjectFactory.CreateEmpty(new Dictionary<CKA, IAttributeValue>
        {
            { CKA.CKA_CLASS, AttributeValue.Create((uint)CKO.CKO_PRIVATE_KEY) },
            { CKA.CKA_KEY_TYPE, AttributeValue.Create((uint)CKK.CKK_HSS) },
        });
        var pub = StorageObjectFactory.CreateEmpty(new Dictionary<CKA, IAttributeValue>
        {
            { CKA.CKA_CLASS, AttributeValue.Create((uint)CKO.CKO_PUBLIC_KEY) },
            { CKA.CKA_KEY_TYPE, AttributeValue.Create((uint)CKK.CKK_HSS) },
        });

        Assert.IsInstanceOfType<HssPrivateKeyObject>(priv);
        Assert.IsInstanceOfType<HssPublicKeyObject>(pub);
    }

    [TestMethod]
    [DataRow(CKA.CKA_VALUE)]
    [DataRow(CKA.CKA_HSS_LEVELS)]
    [DataRow(CKA.CKA_HSS_LMS_TYPES)]
    [DataRow(CKA.CKA_HSS_LMOTS_TYPES)]
    public void PrivateKey_UpdateKeyMaterial_IsReadOnly(CKA attributeType)
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);
        IAttributeValue value = attributeType switch
        {
            CKA.CKA_VALUE => AttributeValue.Create(obj.CkaValue),
            CKA.CKA_HSS_LEVELS => AttributeValue.Create(obj.CkaHssLevels),
            CKA.CKA_HSS_LMS_TYPES => AttributeValue.Create(obj.CkaHssLmsTypes),
            _ => AttributeValue.Create(obj.CkaHssLmotsTypes)
        };

        AssertRpc(CKR.CKR_ATTRIBUTE_READ_ONLY,
            () => obj.SetValue(attributeType, value, true));
    }

    [TestMethod]
    public void PrivateKey_UpdateLabel_IsAllowed()
    {
        HssPrivateKeyObject obj = CreatePrivate(out _);

        obj.SetValue(CKA.CKA_LABEL, AttributeValue.Create("renamed"), true);

        Assert.AreEqual("renamed", obj.CkaLabel);
    }

    [TestMethod]
    [DataRow(CKA.CKA_VALUE)]
    [DataRow(CKA.CKA_HSS_LEVELS)]
    [DataRow(CKA.CKA_HSS_LMS_TYPE)]
    [DataRow(CKA.CKA_HSS_LMOTS_TYPE)]
    public void PublicKey_UpdateKeyMaterial_IsReadOnly(CKA attributeType)
    {
        HssPublicKeyObject obj = new HssPublicKeyObject();
        obj.SetPublicKey(GenerateKeyPair().Public);
        IAttributeValue value = attributeType switch
        {
            CKA.CKA_VALUE => AttributeValue.Create(obj.CkaValue),
            CKA.CKA_HSS_LEVELS => AttributeValue.Create(obj.CkaHssLevels),
            CKA.CKA_HSS_LMS_TYPE => AttributeValue.Create((uint)obj.CkaHssLmsType),
            _ => AttributeValue.Create((uint)obj.CkaHssLmotsType)
        };

        AssertRpc(CKR.CKR_ATTRIBUTE_READ_ONLY,
            () => obj.SetValue(attributeType, value, true));
    }

    [TestMethod]
    public void PublicKey_CreateKeyMaterial_IsAccepted()
    {
        HssPublicKeyObject source = new HssPublicKeyObject();
        source.SetPublicKey(GenerateKeyPair().Public);

        HssPublicKeyObject obj = new HssPublicKeyObject();
        obj.SetValue(CKA.CKA_VALUE, AttributeValue.Create(source.CkaValue), false);
        obj.SetValue(CKA.CKA_HSS_LEVELS, AttributeValue.Create(source.CkaHssLevels), false);
        obj.SetValue(CKA.CKA_HSS_LMS_TYPE, AttributeValue.Create((uint)source.CkaHssLmsType), false);
        obj.SetValue(CKA.CKA_HSS_LMOTS_TYPE, AttributeValue.Create((uint)source.CkaHssLmotsType), false);
        obj.Validate();

        CollectionAssert.AreEqual(source.CkaValue, obj.CkaValue);
    }

    private static HssPrivateKeyObject CreateFromTemplate(byte[] value, uint? levels, uint[]? lmsTypes, uint[]? lmotsTypes, uint? keysRemaining = null)
    {
        HssPrivateKeyObject obj = new HssPrivateKeyObject();
        if (levels.HasValue)
        {
            obj.SetValue(CKA.CKA_HSS_LEVELS, AttributeValue.Create(levels.Value), false);
        }

        if (lmsTypes != null)
        {
            obj.SetValue(CKA.CKA_HSS_LMS_TYPES, AttributeValue.Create(lmsTypes), false);
        }

        if (lmotsTypes != null)
        {
            obj.SetValue(CKA.CKA_HSS_LMOTS_TYPES, AttributeValue.Create(lmotsTypes), false);
        }

        if (keysRemaining.HasValue)
        {
            obj.SetValue(CKA.CKA_HSS_KEYS_REMAINING, AttributeValue.Create(keysRemaining.Value), false);
        }

        obj.SetValue(CKA.CKA_VALUE, AttributeValue.Create(value), false);
        obj.ReComputeAttributes();
        return obj;
    }

    [TestMethod]
    public void PrivateKey_CreateObjectFlow_MissingLmsTypes_Throws()
    {
        byte[] value = CreatePrivate(out _).CkaValue;
        HssPrivateKeyObject obj = CreateFromTemplate(value, 1u, null, new uint[] { 4 });

        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, () => obj.Validate());
    }

    [TestMethod]
    public void PrivateKey_CreateObjectFlow_MissingLmotsTypes_Throws()
    {
        byte[] value = CreatePrivate(out _).CkaValue;
        HssPrivateKeyObject obj = CreateFromTemplate(value, 1u, new uint[] { 5 }, null);

        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, () => obj.Validate());
    }

    [TestMethod]
    public void PrivateKey_CreateObjectFlow_MissingLevels_DefaultsToOne()
    {
        byte[] value = CreatePrivate(out _).CkaValue;
        HssPrivateKeyObject obj = CreateFromTemplate(value, null, new uint[] { 5 }, new uint[] { 4 });

        obj.Validate();
        Assert.AreEqual(1u, obj.CkaHssLevels);
    }

    [TestMethod]
    public void PrivateKey_CreateObjectFlow_MissingLevelsForTwoLevelKey_Throws()
    {
        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(
            new[]
            {
                new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8),
                new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8)
            },
            new SecureRandom()));
        byte[] value = ((HssPrivateKeyParameters)generator.GenerateKeyPair().Private).GetEncoded();

        HssPrivateKeyObject obj = CreateFromTemplate(value, null, new uint[] { 5, 5 }, new uint[] { 4, 4 });

        AssertRpc(CKR.CKR_ATTRIBUTE_VALUE_INVALID, () => obj.Validate());
    }

    [TestMethod]
    public void PrivateKey_CreateObjectFlow_KeysRemainingInTemplate_IsRecomputed()
    {
        byte[] value = CreatePrivate(out _).CkaValue;
        HssPrivateKeyObject obj = CreateFromTemplate(value, 1u, new uint[] { 5 }, new uint[] { 4 }, keysRemaining: 1000u);

        obj.Validate();
        Assert.AreEqual(32u, obj.CkaHssKeysRemaining);
    }
}
