using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class RsaPublicKeyObject : PublicKeyObject
{
    public byte[] CkaModulus
    {
        get => this.values[CKA.CKA_MODULUS].AsByteArray();
        set => this.values[CKA.CKA_MODULUS] = AttributeValue.Create(value);
    }

    public uint CkaModulusBits
    {
        get => this.values[CKA.CKA_MODULUS_BITS].AsUint();
        set => this.values[CKA.CKA_MODULUS_BITS] = AttributeValue.Create(value);
    }

    public byte[] CkaPublicExponent
    {
        get => this.values[CKA.CKA_PUBLIC_EXPONENT].AsByteArray();
        set => this.values[CKA.CKA_PUBLIC_EXPONENT] = AttributeValue.Create(value);
    }

    public RsaPublicKeyObject(CKM genMechanism)
        : base(CKK.CKK_RSA, genMechanism)
    {
        this.CkaModulus = Array.Empty<byte>();
        this.CkaModulusBits = 0;
        this.CkaPublicExponent = Array.Empty<byte>();
    }

    internal RsaPublicKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override AsymmetricKeyParameter GetPublicKey()
    {
        return new RsaKeyParameters(false,
            new BigInteger(1, this.CkaModulus),
            new BigInteger(1, this.CkaPublicExponent));
    }

    public override void SetPublicKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is not RsaKeyParameters)
        {
            throw new ArgumentException("Argument is not RsaKeyParameters (public key).", nameof(publicKey));
        }

        RsaKeyParameters rsaKey = (RsaKeyParameters)publicKey;

        this.CkaModulus = rsaKey.Modulus.ToByteArrayUnsigned();
        this.CkaPublicExponent = rsaKey.Exponent.ToByteArrayUnsigned();
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_MODULUS, this.CkaModulus);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_PUBLIC_EXPONENT, this.CkaPublicExponent);
    }
}