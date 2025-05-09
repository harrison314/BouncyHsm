using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.EdEC;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class EdwardsPrivateKeyObject : PrivateKeyObject
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

    public EdwardsPrivateKeyObject(CKM keyGeneMechanism)
        : base(CKK.CKK_EC_EDWARDS, keyGeneMechanism)
    {
        this.CkaEcParams = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();
    }

    internal EdwardsPrivateKeyObject(StorageObjectMemento memento)
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
        DerObjectIdentifier oid = EdEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_Ed25519))
        {
            return new Ed25519PrivateKeyParameters(this.CkaValue);
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_Ed448))
        {
            return new Ed448PrivateKeyParameters(this.CkaValue);
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public ED key.");
        }
    }

    public override void Validate()
    {
        base.Validate();

        DerObjectIdentifier oid = EdEcUtils.GetOidFromParams(this.CkaEcParams);
        if (oid.Equals(EdECObjectIdentifiers.id_Ed25519))
        {
            if (this.CkaValue.Length != Ed25519PrivateKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_VALUE is not valid in EdwardsPrivateKey with id {this.Id}.");
            }
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_Ed448))
        {
            if (this.CkaValue.Length != Ed448PrivateKeyParameters.KeySize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                    $"CKA_VALUE is not valid in EdwardsPrivateKey with id {this.Id}.");
            }
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public ED key.");
        }
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return this.CkaSensitive && attributeType == CKA.CKA_VALUE;
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is Ed25519PrivateKeyParameters ed25519)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_Ed25519.GetEncoded();
            this.CkaValue = ed25519.GetEncoded();
        }
        else if (privateKey is Ed448PrivateKeyParameters ed448)
        {
            this.CkaEcParams = EdECObjectIdentifiers.id_Ed448.GetEncoded();
            this.CkaValue = ed448.GetEncoded();
        }
        else
        {
            throw new ArgumentException("privateKey is not edwards key parameters", nameof(privateKey));
        }
    }
}
