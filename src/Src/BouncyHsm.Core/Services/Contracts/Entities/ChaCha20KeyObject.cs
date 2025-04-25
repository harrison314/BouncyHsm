using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class ChaCha20KeyObject : SecretKeyObject
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

    public ChaCha20KeyObject()
        : base(CKK.CKK_CHACHA20, CKM.CKM_CHACHA20_KEY_GEN)
    {
        this.CkaValue = Array.Empty<byte>();
        this.CkaValueLen = 0;
    }

    public ChaCha20KeyObject(StorageObjectMemento memento)
        : base(memento)
    {

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
        return 32U;
    }

    public override uint? GetRequiredSecretLen()
    {
        return 32U;
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

        this.CkaCheckValue = DigestUtils.ComputeCheckValue(this.CkaValue);
    }

    public override void Validate()
    {
        base.Validate();

        if (this.CkaValueLen != (uint)this.CkaValue.Length)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
              $"Attribute {CKA.CKA_VALUE} has different lenth than {CKA.CKA_VALUE_LEN} value.");
        }

        if (this.CkaValueLen != 32)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
              $"Attribute {CKA.CKA_VALUE} has no valid lenght for CHACHA20 (32B) key.");
        }
    }

    public override string ToString()
    {
        return $"{this.GetType().Name} (CHACHA20): Id={this.Id}";
    }

    public override void SetValue(CKA attributeType, IAttributeValue value, bool isUpdating)
    {
        if (attributeType == CKA.CKA_KEY_TYPE)
        {
            if (value.Equals((uint)CKK.CKK_CHACHA20))
            {
                return;
            }
            else
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {CKA.CKA_KEY_TYPE} is not {CKK.CKK_CHACHA20}.");
            }
        }

        base.SetValue(attributeType, value, isUpdating);
    }
}