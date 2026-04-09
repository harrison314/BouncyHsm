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

        this.CkaCheckValue = DigestUtils.ComputeCheckValue(this.CkaValue);
    }

    public override void Validate()
    {
        base.Validate();

        if (!this.IsKeyTypeSupported(this.CkaKeyType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
               $"Attribute {CKA.CKA_KEY_TYPE} is not generic seecrit key type.");
        }

        if (this.CkaValueLen < 1)
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

            CKK.CKK_SHA512_224_HMAC => true,
            CKK.CKK_SHA512_256_HMAC => true,
            CKK.CKK_SHA3_224_HMAC => true,
            CKK.CKK_SHA3_256_HMAC => true,
            CKK.CKK_SHA3_384_HMAC => true,
            CKK.CKK_SHA3_512_HMAC => true,
            CKK.CKK_SHA512_T_HMAC => true,

            CKK.CKK_BLAKE2B_160_HMAC => true,
            CKK.CKK_BLAKE2B_256_HMAC => true,
            CKK.CKK_BLAKE2B_384_HMAC => true,
            CKK.CKK_BLAKE2B_512_HMAC => true,

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

            CKK.CKK_SHA512_224_HMAC => 28,
            CKK.CKK_SHA512_256_HMAC => 32,
            CKK.CKK_SHA3_224_HMAC => 28,
            CKK.CKK_SHA3_256_HMAC => 32,
            CKK.CKK_SHA3_384_HMAC => 48,
            CKK.CKK_SHA3_512_HMAC => 64,
            CKK.CKK_SHA512_T_HMAC => 28,

            CKK.CKK_BLAKE2B_160_HMAC => 20,
            CKK.CKK_BLAKE2B_256_HMAC => 32,
            CKK.CKK_BLAKE2B_384_HMAC => 48,
            CKK.CKK_BLAKE2B_512_HMAC => 64,

            _ => throw new InvalidProgramException($"Enum value {keyType} is not supported.")
        };
    }

    protected override CKM[] GetAllovedMechanism()
    {
        return new CKM[]
        {
            CKM.CKM_SHA_1_KEY_GEN,
            CKM.CKM_SHA224_KEY_GEN,
            CKM.CKM_SHA256_KEY_GEN,
            CKM.CKM_SHA384_KEY_GEN,
            CKM.CKM_SHA512_KEY_GEN,
            CKM.CKM_SHA512_224_KEY_GEN,
            CKM.CKM_SHA512_256_KEY_GEN,
            CKM.CKM_SHA512_T_KEY_GEN,
            CKM.CKM_SHA3_224_KEY_GEN,
            CKM.CKM_SHA3_256_KEY_GEN,
            CKM.CKM_SHA3_384_KEY_GEN,
            CKM.CKM_SHA3_512_KEY_GEN,
            CKM.CKM_BLAKE2B_160_KEY_GEN,
            CKM.CKM_BLAKE2B_256_KEY_GEN,
            CKM.CKM_BLAKE2B_384_KEY_GEN,
            CKM.CKM_BLAKE2B_512_KEY_GEN,
            CKM.CKM_MD2_HMAC,
            CKM.CKM_MD5_HMAC,
            CKM.CKM_RIPEMD128_HMAC,
            CKM.CKM_RIPEMD160_HMAC,
            CKM.CKM_SHA_1_HMAC,
            CKM.CKM_SHA224_HMAC,
            CKM.CKM_SHA256_HMAC,
            CKM.CKM_SHA384_HMAC,
            CKM.CKM_SHA512_HMAC,
            CKM.CKM_SHA512_224_HMAC,
            CKM.CKM_SHA512_256_HMAC,
            CKM.CKM_GOSTR3411_HMAC,
            CKM.CKM_SHA3_256_HMAC,
            CKM.CKM_SHA3_224_HMAC,
            CKM.CKM_SHA3_384_HMAC,
            CKM.CKM_SHA3_512_HMAC,
            CKM.CKM_BLAKE2B_160_HMAC,
            CKM.CKM_BLAKE2B_256_HMAC,
            CKM.CKM_BLAKE2B_384_HMAC,
            CKM.CKM_BLAKE2B_512_HMAC,
            CKM.CKM_MD2_HMAC_GENERAL,
            CKM.CKM_MD5_HMAC_GENERAL,
            CKM.CKM_RIPEMD128_HMAC_GENERAL,
            CKM.CKM_RIPEMD160_HMAC_GENERAL,
            CKM.CKM_SHA_1_HMAC_GENERAL,
            CKM.CKM_SHA224_HMAC_GENERAL,
            CKM.CKM_SHA256_HMAC_GENERAL,
            CKM.CKM_SHA384_HMAC_GENERAL,
            CKM.CKM_SHA512_HMAC_GENERAL,
            CKM.CKM_SHA512_224_HMAC_GENERAL,
            CKM.CKM_SHA512_256_HMAC_GENERAL,
            CKM.CKM_SHA3_256_HMAC_GENERAL,
            CKM.CKM_SHA3_224_HMAC_GENERAL,
            CKM.CKM_SHA3_384_HMAC_GENERAL,
            CKM.CKM_SHA3_512_HMAC_GENERAL,
            CKM.CKM_BLAKE2B_160_HMAC_GENERAL,
            CKM.CKM_BLAKE2B_256_HMAC_GENERAL,
            CKM.CKM_BLAKE2B_384_HMAC_GENERAL,
            CKM.CKM_BLAKE2B_512_HMAC_GENERAL,
            CKM.CKM_AES_CMAC_GENERAL,
            CKM.CKM_MD2_KEY_DERIVATION,
            CKM.CKM_MD5_KEY_DERIVATION,
            CKM.CKM_SHA1_KEY_DERIVATION,
            CKM.CKM_SHA224_KEY_DERIVATION,
            CKM.CKM_SHA256_KEY_DERIVATION,
            CKM.CKM_SHA384_KEY_DERIVATION,
            CKM.CKM_SHA512_KEY_DERIVATION,
            CKM.CKM_SHA512_224_KEY_DERIVATION,
            CKM.CKM_SHA512_256_KEY_DERIVATION,
            CKM.CKM_SHA3_256_KEY_DERIVATION,
            CKM.CKM_SHA3_224_KEY_DERIVATION,
            CKM.CKM_SHA3_384_KEY_DERIVATION,
            CKM.CKM_SHA3_512_KEY_DERIVATION,
            CKM.CKM_BLAKE2B_160_KEY_DERIVE,
            CKM.CKM_BLAKE2B_256_KEY_DERIVE,
            CKM.CKM_BLAKE2B_384_KEY_DERIVE,
            CKM.CKM_BLAKE2B_512_KEY_DERIVE,
            CKM.CKM_SHAKE_128_KEY_DERIVATION,
            CKM.CKM_SHAKE_256_KEY_DERIVATION,
            CKM.CKM_HKDF_DERIVE,
            CKM.CKM_CONCATENATE_BASE_AND_DATA,
            CKM.CKM_CONCATENATE_DATA_AND_BASE,
            CKM.CKM_XOR_BASE_AND_DATA,
            CKM.CKM_CONCATENATE_BASE_AND_KEY,
            CKM.CKM_EXTRACT_KEY_FROM_KEY,
        };
    }
}