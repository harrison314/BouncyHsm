using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class MontgomeryPrivateKeyObject : PrivateKeyObject
{
    public byte[] CkaEcParams
    {
        get => this.values[CKA.CKA_EC_PARAMS].AsByteArray();
        set => this.values[CKA.CKA_EC_PARAMS] = AttributeValue.Create(value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public MontgomeryPrivateKeyObject(CKM keyGeneMechanism)
        : base(CKK.CKK_EC_MONTGOMERY, keyGeneMechanism)
    {
        this.CkaEcParams = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();
    }

    internal MontgomeryPrivateKeyObject(StorageObjectMemento memento)
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
        DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_X25519))
        {
            return new X25519PrivateKeyParameters(this.CkaValue);
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_X448))
        {
            return new X448PrivateKeyParameters(this.CkaValue);
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public montgomery key.");
        }
    }

    public override void Validate()
    {
        base.Validate();

        DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_X25519))
        {
            if (this.CkaValue.Length != X25519PrivateKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_VALUE is not valid in MontgomeryPrivateKeyObject with id {this.Id}.");
            }
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_X448))
        {
            if (this.CkaValue.Length != X448PrivateKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_VALUE is not valid in MontgomeryPrivateKeyObject with id {this.Id}.");
            }
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public montgomery key.");
        }
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return this.CkaSensitive && attributeType == CKA.CKA_VALUE;
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is X25519PrivateKeyParameters x25519)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_X25519.GetEncoded();
            this.CkaValue = x25519.GetEncoded();
        }
        else if (privateKey is X448PrivateKeyParameters x448)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_X448.GetEncoded();
            this.CkaValue = x448.GetEncoded();
        }
        else
        {
            throw new ArgumentException("privateKey is not montgomery key parameters", nameof(privateKey));
        }
    }
}
