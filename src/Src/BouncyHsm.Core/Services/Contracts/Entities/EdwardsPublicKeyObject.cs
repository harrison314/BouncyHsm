using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class EdwardsPublicKeyObject : PublicKeyObject
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

    public EdwardsPublicKeyObject(CKM keyGeneMechanism)
        : base(CKK.CKK_EC_EDWARDS, keyGeneMechanism)
    {
        this.CkaEcParams = Array.Empty<byte>();
        this.CkaEcPoint = Array.Empty<byte>();
    }

    internal EdwardsPublicKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override AsymmetricKeyParameter GetPublicKey()
    {
        DerObjectIdentifier oid = EdEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_Ed25519))
        {
            return new Ed25519PublicKeyParameters(this.CkaEcPoint);
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_Ed448))
        {
            return new Ed448PublicKeyParameters(this.CkaEcPoint);
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public ED key.");
        }
    }

    public override void SetPublicKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is Ed25519PublicKeyParameters ed25519)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_Ed25519.GetEncoded();
            this.CkaEcPoint = ed25519.GetEncoded();
        }
        else if (publicKey is Ed448PublicKeyParameters ed448)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_Ed448.GetEncoded();
            this.CkaEcPoint = ed448.GetEncoded();
        }
        else
        {
            throw new ArgumentException("publicKey is not edwards key parameters", nameof(publicKey));
        }
    }

    public override void Validate()
    {
        base.Validate();

        DerObjectIdentifier oid = EdEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_Ed25519))
        {
            if (this.CkaEcPoint.Length != Ed25519PublicKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_EC_POINT is not valid in EdwardsPublicKey.");
            }
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_Ed448))
        {
            if (this.CkaEcPoint.Length != Ed448PublicKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_EC_POINT is not valid in EdwardsPublicKey.");
            }
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public ED key.");
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
