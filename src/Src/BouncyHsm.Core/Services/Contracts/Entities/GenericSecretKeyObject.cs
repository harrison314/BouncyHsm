using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class GenericSecretKeyObject : SecretKeyObject
{
    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public uint CkaValueLen
    {
        get => this.values[CKA.CKA_VALUE_LEN].AsUint();
        set => this.values[CKA.CKA_VALUE_LEN] = AttributeValue.Create(value);
    }

    public GenericSecretKeyObject()
        : base(CKK.CKK_GENERIC_SECRET, CKM.CKM_GENERIC_SECRET_KEY_GEN)
    {
        this.CkaValue = Array.Empty<byte>();
        this.CkaValueLen = 0;
    }

    public GenericSecretKeyObject(StorageObjectMemento memento)
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

    public override void ReComputeAttributes()
    {
        base.ReComputeAttributes();
        if (this.CkaValueLen == 0)
        {
            this.CkaValueLen = (uint)this.CkaValue.Length;
        }

        this.CkaCheckValue = DigestUtils.ComputeCheckValue(this.CkaValue); //TODO: check with specification
    }

    public override void Validate()
    {
        base.Validate();

        if (!this.IsKeyTypeSupported(this.CkaKeyType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
               $"Attribute {CKA.CKA_KEY_TYPE} is not generic seecrit key type.");
        }

        if (this.CkaValueLen < this.GetMinKeySize(this.CkaKeyType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
              $"Attribute {CKA.CKA_VALUE_LEN} is too small for {this.CkaKeyType}.");
        }

        if (this.CkaValueLen != (uint)this.CkaValue.Length)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
              $"Attribute {CKA.CKA_VALUE} has different lenth than {CKA.CKA_VALUE_LEN} value.");
        }
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return this.CkaSensitive && attributeType == CKA.CKA_VALUE;
    }

    public override void SetValue(CKA attributeType, IAttributeValue value, bool isUpdating)
    {
        if (attributeType == CKA.CKA_KEY_TYPE)
        {
            if (!this.IsKeyTypeSupported((CKK)value.AsUint()))
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {CKA.CKA_KEY_TYPE} is not generic seecrit key type.");
            }

            this.values[CKA.CKA_KEY_TYPE] = value;
            return;
        }

        base.SetValue(attributeType, value, isUpdating);
    }

    public override byte[] GetSecret()
    {
        return this.CkaValue;
    }

    public override void SetSecret(byte[] secret)
    {
        System.Diagnostics.Debug.Assert(secret != null);

        this.CkaValue = secret;
        this.CkaValueLen = (uint)secret.Length;
    }

    public override uint GetMinimalSecretLen()
    {
        return this.GetMinKeySize(this.CkaKeyType);
    }

    public override uint? GetRequiredSecretLen()
    {
        return null;
    }

    private bool IsKeyTypeSupported(CKK keyType)
    {
        return keyType switch
        {
            CKK.CKK_GENERIC_SECRET => true,
            CKK.CKK_MD5_HMAC => true,
            CKK.CKK_SHA_1_HMAC => true,
            CKK.CKK_SHA224_HMAC => true,
            CKK.CKK_SHA256_HMAC => true,
            CKK.CKK_SHA384_HMAC => true,
            CKK.CKK_SHA512_HMAC => true,
            CKK.CKK_RIPEMD128_HMAC => true,
            CKK.CKK_RIPEMD160_HMAC => true,
            _ => false
        };
    }

    private uint GetMinKeySize(CKK keyType)
    {
        return keyType switch
        {
            CKK.CKK_GENERIC_SECRET => 1,
            CKK.CKK_MD5_HMAC => 16,
            CKK.CKK_SHA_1_HMAC => 20,
            CKK.CKK_SHA224_HMAC => 28,
            CKK.CKK_SHA256_HMAC => 32,
            CKK.CKK_SHA384_HMAC => 48,
            CKK.CKK_SHA512_HMAC => 64,
            CKK.CKK_RIPEMD128_HMAC => 16,
            CKK.CKK_RIPEMD160_HMAC => 20,
            _ => throw new InvalidProgramException($"Enum value {keyType} is not supported.")
        };
    }
}