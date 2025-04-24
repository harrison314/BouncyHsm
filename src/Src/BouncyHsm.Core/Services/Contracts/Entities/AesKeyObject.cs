using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class AesKeyObject : SecretKeyObject
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

    public AesKeyObject()
        : base(CKK.CKK_AES, CKM.CKM_AES_KEY_GEN)
    {
        this.CkaValue = Array.Empty<byte>();
        this.CkaValueLen = 0;
    }

    public AesKeyObject(StorageObjectMemento memento)
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

        this.CkaCheckValue = this.CreateCheckValue(this.CkaValue);
    }

    public override void Validate()
    {
        base.Validate();

        if (this.CkaValueLen != (uint)this.CkaValue.Length)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
              $"Attribute {CKA.CKA_VALUE} has different lenth than {CKA.CKA_VALUE_LEN} value.");
        }

        if (!IsKeySizeValid((int)this.CkaValueLen))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
              $"Attribute {CKA.CKA_VALUE} has no valid lenght for AES key.");
        }
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return this.CkaSensitive && attributeType == CKA.CKA_VALUE;
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
        return 16;
    }

    public override uint? GetRequiredSecretLen()
    {
        return null;
    }

    public override void SetValue(CKA attributeType, IAttributeValue value, bool isUpdating)
    {
        if (attributeType == CKA.CKA_KEY_TYPE)
        {
            if (value.Equals((uint)CKK.CKK_AES))
            {
                return;
            }
            else
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {CKA.CKA_KEY_TYPE} is not {CKK.CKK_AES}.");
            }
        }

        base.SetValue(attributeType, value, isUpdating);
    }

    public override string ToString()
    {
        return $"{this.GetType().Name} (AES-{this.CkaValueLen * 8}): Id={this.Id}";
    }

    internal static bool IsKeySizeValid(int size)
    {
        return size is 16 or 24 or 32;
    }

    private byte[] CreateCheckValue(byte[] ckaValue)
    {
        EcbBlockCipher cipher = new EcbBlockCipher(AesUtilities.CreateEngine());
        int blockSize = cipher.GetBlockSize();
        cipher.Init(true, new KeyParameter(ckaValue));

        Span<byte> vector = stackalloc byte[blockSize];
        Span<byte> outVector = stackalloc byte[blockSize];

        vector.Fill(0);
        cipher.ProcessBlock(vector, outVector);
        return outVector.Slice(0, 3).ToArray();
    }
}