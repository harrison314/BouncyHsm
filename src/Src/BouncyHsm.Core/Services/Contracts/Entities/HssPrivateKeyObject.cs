using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class HssPrivateKeyObject : PrivateKeyObject
{
    public uint CkaHssLevels
    {
        get => this.values[CKA.CKA_HSS_LEVELS].AsUint();
        set => this.values[CKA.CKA_HSS_LEVELS] = AttributeValue.Create(value);
    }

    public uint[] CkaHssLmsTypes
    {
        get => this.values[CKA.CKA_HSS_LMS_TYPES].AsUintArray();
        set => this.values[CKA.CKA_HSS_LMS_TYPES] = AttributeValue.Create(value);
    }

    public uint[] CkaHssLmotsTypes
    {
        get => this.values[CKA.CKA_HSS_LMOTS_TYPES].AsUintArray();
        set => this.values[CKA.CKA_HSS_LMOTS_TYPES] = AttributeValue.Create(value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public uint CkaHssKeysRemaining
    {
        get => this.values[CKA.CKA_HSS_KEYS_REMAINING].AsUint();
        set => this.values[CKA.CKA_HSS_KEYS_REMAINING] = AttributeValue.Create(value);
    }

    public HssPrivateKeyObject()
        : base(CKK.CKK_HSS, CKM.CKM_HSS_KEY_PAIR_GEN)
    {
        this.CkaSensitive = true;
        this.CkaExtractable = false;
        this.CkaCopyable = false;

        this.CkaHssLevels = HssUtils.MinLevels;
        this.CkaHssLmsTypes = Array.Empty<uint>();
        this.CkaHssLmotsTypes = Array.Empty<uint>();
        this.CkaValue = Array.Empty<byte>();
        this.CkaHssKeysRemaining = 0;
    }

    internal HssPrivateKeyObject(StorageObjectMemento memento)
        : base(memento)
    {
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override AsymmetricKeyParameter GetPrivateKey()
    {
        return HssPrivateKeyParameters.GetInstance(this.CkaValue);
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is HssPrivateKeyParameters hssPrivateKey)
        {
            LmsParameters[] parameters = hssPrivateKey.GetLmsParameters();

            this.CkaValue = hssPrivateKey.GetEncoded();
            this.CkaHssLevels = (uint)hssPrivateKey.Level;
            this.CkaHssLmsTypes = parameters.Select(p => (uint)HssUtils.GetLmsType(p.LMSigParameters)).ToArray();
            this.CkaHssLmotsTypes = parameters.Select(p => (uint)HssUtils.GetLmOtsType(p.LMOtsParameters)).ToArray();
            this.CkaHssKeysRemaining = HssUtils.ToKeysRemaining(hssPrivateKey.GetUsagesRemaining());
        }
        else
        {
            throw new ArgumentException("privateKey is not HssPrivateKeyParameters", nameof(privateKey));
        }
    }

    public override void SetValue(CKA attributeType, IAttributeValue value, bool isUpdating)
    {
        // Key material is immutable after creation: rewriting CKA_VALUE could rewind the HSS state.
        if (isUpdating && attributeType is CKA.CKA_VALUE
            or CKA.CKA_HSS_LEVELS
            or CKA.CKA_HSS_LMS_TYPES
            or CKA.CKA_HSS_LMOTS_TYPES)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_READ_ONLY,
                $"Attribute {attributeType} in object {this.GetType().Name} is read only.");
        }

        base.SetValue(attributeType, value, isUpdating);
    }

    public override void ReComputeAttributes()
    {
        base.ReComputeAttributes();

        byte[] value = this.CkaValue;
        if (value.Length == 0)
        {
            return;
        }

        try
        {
            HssPrivateKeyParameters decoded = HssPrivateKeyParameters.GetInstance(value);
            this.CkaHssKeysRemaining = HssUtils.ToKeysRemaining(decoded.GetUsagesRemaining());
        }
        catch (Exception)
        {
            // Invalid value is reported by Validate.
        }
    }

    public override void Validate()
    {
        base.Validate();

        if (!this.CkaSensitive || this.CkaExtractable || this.CkaCopyable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                "HSS private key must have CKA_SENSITIVE=true, CKA_EXTRACTABLE=false and CKA_COPYABLE=false.");
        }

        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);

        HssPrivateKeyParameters decoded;
        try
        {
            decoded = HssPrivateKeyParameters.GetInstance(this.CkaValue);
        }
        catch (Exception)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_VALUE is not valid HSS private key.");
        }

        LmsParameters[] parameters = decoded.GetLmsParameters();
        if (this.CkaHssLevels != (uint)decoded.Level)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_HSS_LEVELS does not match CKA_VALUE.");
        }

        uint[] lmsTypes = this.CkaHssLmsTypes;
        uint[] lmotsTypes = this.CkaHssLmotsTypes;
        if (lmsTypes.Length != parameters.Length || lmotsTypes.Length != parameters.Length)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attributes CKA_HSS_LMS_TYPES and CKA_HSS_LMOTS_TYPES do not match CKA_VALUE.");
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            if (lmsTypes[i] != (uint)parameters[i].LMSigParameters.ID)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_HSS_LMS_TYPES does not match CKA_VALUE.");
            }

            if (lmotsTypes[i] != (uint)parameters[i].LMOtsParameters.ID)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_HSS_LMOTS_TYPES does not match CKA_VALUE.");
            }
        }
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return (this.CkaSensitive || !this.CkaExtractable) && attributeType == CKA.CKA_VALUE;
    }

    protected override bool IsReadOnlyAttribute(CKA attributeType)
    {
        return attributeType == CKA.CKA_HSS_KEYS_REMAINING || base.IsReadOnlyAttribute(attributeType);
    }

    protected override CKM[] GetAllovedMechanism()
    {
        return AllovedMechanismLookup.HssKeyObject;
    }
}
