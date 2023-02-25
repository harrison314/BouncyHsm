using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace BouncyHsm.Core.Services.Contracts.Entities;

// See https://www.cryptsoft.com/pkcs11doc/v201/group__SEC__10__7__3__ECDSA__PRIVATE__KEY__OBJECTS.html
public sealed class EcdsaPrivateKeyObject : PrivateKeyObject
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

    public EcdsaPrivateKeyObject(CKM keyGeneMechanism)
        : base(CKK.CKK_ECDSA, keyGeneMechanism)
    {
        this.CkaEcParams = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();
    }

    internal EcdsaPrivateKeyObject(StorageObjectMemento memento)
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
        Org.BouncyCastle.Asn1.X9.X9ECParameters ecParams = EcdsaUtils.ParseEcParams(this.CkaEcParams);

        BigInteger d = new BigInteger(1, this.CkaValue);
        return new ECPrivateKeyParameters(d, new ECDomainParameters(ecParams));
    }

    public override void Validate()
    {
        base.Validate();

        _ = EcdsaUtils.ParseEcParams(this.CkaEcParams);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return this.CkaSensitive && attributeType == CKA.CKA_VALUE;
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is not ECPrivateKeyParameters)
        {
            throw new ArgumentException("private key is not ECPrivateKeyParameters", nameof(privateKey));
        }

        ECPrivateKeyParameters ecdsaPrivateKey = (ECPrivateKeyParameters)privateKey;
        this.CkaValue = ecdsaPrivateKey.D.ToByteArrayUnsigned();
        this.CkaEcParams = ecdsaPrivateKey.PublicKeyParamSet.GetEncoded();
    }
}