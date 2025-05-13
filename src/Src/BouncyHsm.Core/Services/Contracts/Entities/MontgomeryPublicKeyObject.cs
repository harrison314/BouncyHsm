using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class MontgomeryPublicKeyObject : PublicKeyObject
{
    public byte[] CkaEcParams
    {
        get => this.values[CKA.CKA_EC_PARAMS].AsByteArray();
        set => this.values[CKA.CKA_EC_PARAMS] = AttributeValue.Create(value);
    }

    public byte[] CkaEcPoint
    {
        get => this.values[CKA.CKA_EC_POINT].AsByteArray();
        set => this.values[CKA.CKA_EC_POINT] = AttributeValue.Create(value);
    }

    public MontgomeryPublicKeyObject(CKM keyGeneMechanism)
        : base(CKK.CKK_EC_MONTGOMERY, keyGeneMechanism)
    {
        this.CkaEcParams = Array.Empty<byte>();
        this.CkaEcPoint = Array.Empty<byte>();
    }

    internal MontgomeryPublicKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override AsymmetricKeyParameter GetPublicKey()
    {
        DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_X25519))
        {
            return new X25519PublicKeyParameters(this.CkaEcPoint);
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_X448))
        {
            return new X448PublicKeyParameters(this.CkaEcPoint);
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public montgomery key.");
        }
    }

    public override void SetPublicKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is X25519PublicKeyParameters x25519)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_X25519.GetEncoded();
            this.CkaEcPoint = x25519.GetEncoded();
        }
        else if (publicKey is X448PublicKeyParameters x448)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_X448.GetEncoded();
            this.CkaEcPoint = x448.GetEncoded();
        }
        else
        {
            throw new ArgumentException("publicKey is not montgomery key parameters", nameof(publicKey));
        }
    }

    public override void Validate()
    {
        base.Validate();

        DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_X25519))
        {
            if (this.CkaEcPoint.Length != X25519PublicKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_EC_POINT is not valid in MontgomeryPublicKeyObject.");
            }
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_X448))
        {
            if (this.CkaEcPoint.Length != X448PublicKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_EC_POINT is not valid in MontgomeryPublicKeyObject.");
            }
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public montgomery key.");
        }
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
