using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Security.Cryptography.X509Certificates;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class RsaPrivateKeyObject : PrivateKeyObject
{
    public byte[] CkaModulus
    {
        get => this.values[CKA.CKA_MODULUS].AsByteArray();
        set => this.values[CKA.CKA_MODULUS] = AttributeValue.Create(value);
    }

    public byte[] CkaPublicExponent
    {
        get => this.values[CKA.CKA_PUBLIC_EXPONENT].AsByteArray();
        set => this.values[CKA.CKA_PUBLIC_EXPONENT] = AttributeValue.Create(value);
    }

    public byte[] CkaPrivateExponent
    {
        get => this.values[CKA.CKA_PRIVATE_EXPONENT].AsByteArray();
        set => this.values[CKA.CKA_PRIVATE_EXPONENT] = AttributeValue.Create(value);
    }

    public byte[] CkaPrime1
    {
        get => this.values[CKA.CKA_PRIME_1].AsByteArray();
        set => this.values[CKA.CKA_PRIME_1] = AttributeValue.Create(value);
    }

    public byte[] CkaPrime2
    {
        get => this.values[CKA.CKA_PRIME_2].AsByteArray();
        set => this.values[CKA.CKA_PRIME_2] = AttributeValue.Create(value);
    }

    public byte[] CkaExponent1
    {
        get => this.values[CKA.CKA_EXPONENT_1].AsByteArray();
        set => this.values[CKA.CKA_EXPONENT_1] = AttributeValue.Create(value);
    }

    public byte[] CkaExponent2
    {
        get => this.values[CKA.CKA_EXPONENT_2].AsByteArray();
        set => this.values[CKA.CKA_EXPONENT_2] = AttributeValue.Create(value);
    }

    public byte[] CkaCoefficient
    {
        get => this.values[CKA.CKA_COEFFICIENT].AsByteArray();
        set => this.values[CKA.CKA_COEFFICIENT] = AttributeValue.Create(value);
    }

    public RsaPrivateKeyObject(CKM genMechanism)
        : base(CKK.CKK_RSA, genMechanism)
    {
        this.CkaModulus = Array.Empty<byte>();
        this.CkaPublicExponent = Array.Empty<byte>();
        this.CkaPrivateExponent = Array.Empty<byte>();
        this.CkaPrime1 = Array.Empty<byte>();
        this.CkaPrime2 = Array.Empty<byte>();
        this.CkaExponent1 = Array.Empty<byte>();
        this.CkaExponent2 = Array.Empty<byte>();
        this.CkaCoefficient = Array.Empty<byte>();
    }

    internal RsaPrivateKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override AsymmetricKeyParameter GetPrivateKey()
    {
        return new RsaPrivateCrtKeyParameters(new BigInteger(1, this.CkaModulus),
            new BigInteger(1, this.CkaPublicExponent),
            new BigInteger(1, this.CkaPrivateExponent),
            new BigInteger(1, this.CkaPrime1),
            new BigInteger(1, this.CkaPrime2),
            new BigInteger(1, this.CkaExponent1),
            new BigInteger(1, this.CkaExponent2),
            new BigInteger(1, this.CkaCoefficient));
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is not AsymmetricKeyParameter)
        {
            throw new ArgumentException("Argument is not AsymmetricKeyParameter (private key).", nameof(privateKey));
        }

        RsaPrivateCrtKeyParameters rsaPrivateKey = (RsaPrivateCrtKeyParameters)privateKey;

        this.CkaModulus = rsaPrivateKey.Modulus.ToByteArrayUnsigned();
        this.CkaPublicExponent = rsaPrivateKey.PublicExponent.ToByteArrayUnsigned();
        this.CkaPrivateExponent = rsaPrivateKey.Exponent.ToByteArrayUnsigned();
        this.CkaPrime1 = rsaPrivateKey.P.ToByteArrayUnsigned();
        this.CkaPrime2 = rsaPrivateKey.Q.ToByteArrayUnsigned();
        this.CkaExponent1 = rsaPrivateKey.DP.ToByteArrayUnsigned();
        this.CkaExponent2 = rsaPrivateKey.DQ.ToByteArrayUnsigned();
        this.CkaCoefficient = rsaPrivateKey.QInv.ToByteArrayUnsigned();
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    protected override bool IsSensitiveAttribute(CKA attributeType)
    {
        return this.CkaSensitive
            && attributeType is CKA.CKA_PRIVATE_EXPONENT
              or CKA.CKA_PRIME_1
              or CKA.CKA_PRIME_2
              or CKA.CKA_EXPONENT_1
              or CKA.CKA_EXPONENT_2
              or CKA.CKA_COEFFICIENT;
    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_MODULUS, this.CkaModulus);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_PUBLIC_EXPONENT, this.CkaPublicExponent);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_PRIVATE_EXPONENT, this.CkaPrivateExponent);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_PRIME_1, this.CkaPrime1);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_PRIME_2, this.CkaPrime2);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_EXPONENT_1, this.CkaExponent1);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_EXPONENT_2, this.CkaExponent2);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_COEFFICIENT, this.CkaCoefficient);
    }
}