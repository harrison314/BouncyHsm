using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class HssPublicKeyObject : PublicKeyObject
{
    public uint CkaHssLevels
    {
        get => this.values[CKA.CKA_HSS_LEVELS].AsUint();
        set => this.values[CKA.CKA_HSS_LEVELS] = AttributeValue.Create(value);
    }

    public CK_LMS_TYPE CkaHssLmsType
    {
        get => (CK_LMS_TYPE)this.values[CKA.CKA_HSS_LMS_TYPE].AsUint();
        set => this.values[CKA.CKA_HSS_LMS_TYPE] = AttributeValue.Create((uint)value);
    }

    public CK_LMOTS_TYPE CkaHssLmotsType
    {
        get => (CK_LMOTS_TYPE)this.values[CKA.CKA_HSS_LMOTS_TYPE].AsUint();
        set => this.values[CKA.CKA_HSS_LMOTS_TYPE] = AttributeValue.Create((uint)value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    internal HssPublicKeyObject(StorageObjectMemento memento)
        : base(memento)
    {
    }

    public HssPublicKeyObject()
        : base(CKK.CKK_HSS, CKM.CKM_HSS_KEY_PAIR_GEN)
    {
        this.CkaHssLevels = 0;
        this.CkaHssLmsType = 0;
        this.CkaHssLmotsType = 0;
        this.CkaValue = Array.Empty<byte>();
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override AsymmetricKeyParameter GetPublicKey()
    {
        return HssPublicKeyParameters.GetInstance(this.CkaValue);
    }

    public override void SetPublicKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is HssPublicKeyParameters hssPublicKey)
        {
            this.CkaValue = hssPublicKey.GetEncoded();
            this.CkaHssLevels = (uint)hssPublicKey.Level;
            this.CkaHssLmsType = HssUtils.GetLmsType(hssPublicKey.LmsPublicKey.GetSigParameters());
            this.CkaHssLmotsType = HssUtils.GetLmOtsType(hssPublicKey.LmsPublicKey.GetOtsParameters());
        }
        else
        {
            throw new ArgumentException("publicKey is not HssPublicKeyParameters", nameof(publicKey));
        }
    }

    public override void SetValue(CKA attributeType, IAttributeValue value, bool isUpdating)
    {
        // Key material is immutable after creation: rewriting CKA_VALUE could rewind the HSS state.
        if (isUpdating && attributeType is CKA.CKA_VALUE
            or CKA.CKA_HSS_LEVELS
            or CKA.CKA_HSS_LMS_TYPE
            or CKA.CKA_HSS_LMOTS_TYPE)
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

        HssPublicKeyParameters? decoded = this.TryDecodeHssPublicKeyParameters(value);
        if (decoded == null)
        {
            return;
        }

        if (this.CkaHssLevels == 0)
        {
            this.CkaHssLevels = (uint)decoded.Level;
        }

        if (this.CkaHssLmsType == 0)
        {
            this.CkaHssLmsType = (CK_LMS_TYPE)decoded.LmsPublicKey.GetSigParameters().ID;
        }

        if (this.CkaHssLmotsType == 0)
        {
            this.CkaHssLmotsType = (CK_LMOTS_TYPE)decoded.LmsPublicKey.GetOtsParameters().ID;
        }
    }

    public override void Validate()
    {
        base.Validate();
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);

        HssPublicKeyParameters? decoded = this.TryDecodeHssPublicKeyParameters(this.CkaValue);
        if (decoded == null)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_VALUE is not valid HSS public key.");
        }

        if (this.CkaHssLevels != (uint)decoded.Level)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_HSS_LEVELS does not match CKA_VALUE.");
        }

        if ((uint)this.CkaHssLmsType != (uint)decoded.LmsPublicKey.GetSigParameters().ID)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_HSS_LMS_TYPE does not match CKA_VALUE.");
        }

        if ((uint)this.CkaHssLmotsType != (uint)decoded.LmsPublicKey.GetOtsParameters().ID)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Attribute CKA_HSS_LMOTS_TYPE does not match CKA_VALUE.");
        }
    }

    protected override CKM[] GetAllovedMechanism()
    {
        return AllovedMechanismLookup.HssKeyObject;
    }

    private HssPublicKeyParameters? TryDecodeHssPublicKeyParameters(byte[] value)
    {
        try
        {
            return HssPublicKeyParameters.GetInstance(value);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
