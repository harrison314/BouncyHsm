using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

// See https://www.cryptsoft.com/pkcs11doc/v230/group__SEC__11__3__3__ECDSA__PUBLIC__KEY__OBJECTS.html
public sealed class EcdsaPublicKeyObject : PublicKeyObject
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

    public EcdsaPublicKeyObject(CKM keyGeneMechanism)
        : base(CKK.CKK_ECDSA, keyGeneMechanism)
    {
        this.CkaEcParams = Array.Empty<byte>();
        this.CkaEcPoint = Array.Empty<byte>();
    }

    internal EcdsaPublicKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override AsymmetricKeyParameter GetPublicKey()
    {
        Org.BouncyCastle.Asn1.X9.X9ECParameters ecParams = EcdsaUtils.ParseEcParams(this.CkaEcParams);

        return new ECPublicKeyParameters(EcdsaUtils.DecodeP11EcPoint(ecParams, this.CkaEcPoint),
            EcdsaUtils.ParseECDomainParameters(this.CkaEcParams));
    }

    public override void SetPublicKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is not ECPublicKeyParameters)
        {
            throw new ArgumentException("publicKey is not ECPublicKeyParameters", nameof(publicKey));
        }

        ECPublicKeyParameters ecdsaPublicKey = (ECPublicKeyParameters)publicKey;
        this.CkaEcPoint = EcdsaUtils.EncodeP11EcPoint(ecdsaPublicKey.Q);

        if (ecdsaPublicKey.PublicKeyParamSet != null)
        {
            this.CkaEcParams = ecdsaPublicKey.PublicKeyParamSet.GetEncoded();
        }
        else
        {
            Org.BouncyCastle.Asn1.X9.X9ECParameters x9EcParameters = new Org.BouncyCastle.Asn1.X9.X9ECParameters(ecdsaPublicKey.Parameters.Curve,
                new Org.BouncyCastle.Asn1.X9.X9ECPoint(ecdsaPublicKey.Parameters.G, false),
                ecdsaPublicKey.Parameters.N,
                ecdsaPublicKey.Parameters.H,
                ecdsaPublicKey.Parameters.GetSeed());
            this.CkaEcParams = x9EcParameters.GetEncoded();
        }
    }

    public override void Validate()
    {
        base.Validate();

        ECPublicKeyParameters pubKey = (ECPublicKeyParameters)this.GetPublicKey();
        if (!pubKey.Q.IsValid())
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"CKA_EC_POINT is not valid in EcdsaPublicKeyObject with id {this.Id}.");
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
