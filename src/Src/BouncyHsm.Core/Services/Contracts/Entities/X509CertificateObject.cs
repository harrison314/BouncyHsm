using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class X509CertificateObject : CertificateObject
{
    public override CKC CkaCertificateType
    {
        get => CKC.CKC_X_509;
    }

    public byte[] CkaSubject
    {
        get => this.values[CKA.CKA_SUBJECT].AsByteArray();
        set => this.values[CKA.CKA_SUBJECT] = AttributeValue.Create(value);
    }

    public byte[] CkaId
    {
        get => this.values[CKA.CKA_ID].AsByteArray();
        set => this.values[CKA.CKA_ID] = AttributeValue.Create(value);
    }

    public byte[] CkaIssuer
    {
        get => this.values[CKA.CKA_ISSUER].AsByteArray();
        set => this.values[CKA.CKA_ISSUER] = AttributeValue.Create(value);
    }

    public byte[] CkaSerialNumber
    {
        get => this.values[CKA.CKA_SERIAL_NUMBER].AsByteArray();
        set => this.values[CKA.CKA_SERIAL_NUMBER] = AttributeValue.Create(value);
    }

    /// <summary>
    /// BER-encoding of the certificate
    /// </summary>
    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public string CkaUrl
    {
        get => this.values[CKA.CKA_URL].AsString();
        set => this.values[CKA.CKA_URL] = AttributeValue.Create(value);
    }

    public byte[] CkaHashOfSubjectPublicKey
    {
        get => this.values[CKA.CKA_HASH_OF_SUBJECT_PUBLIC_KEY].AsByteArray();
        set => this.values[CKA.CKA_HASH_OF_SUBJECT_PUBLIC_KEY] = AttributeValue.Create(value);
    }

    public byte[] CkaHashOfIssuerPublicKey
    {
        get => this.values[CKA.CKA_HASH_OF_ISSUER_PUBLIC_KEY].AsByteArray();
        set => this.values[CKA.CKA_HASH_OF_ISSUER_PUBLIC_KEY] = AttributeValue.Create(value);
    }

    public uint CkaJavaMidpSecurityDomain //TODO: use enum?
    {
        get => this.values[CKA.CKA_JAVA_MIDP_SECURITY_DOMAIN].AsUint();
        set => this.values[CKA.CKA_JAVA_MIDP_SECURITY_DOMAIN] = AttributeValue.Create(value);
    }

    public CKM CkaNameHashAlgorithm
    {
        get => (CKM)this.values[CKA.CKA_NAME_HASH_ALGORITHM].AsUint();
        set => this.values[CKA.CKA_NAME_HASH_ALGORITHM] = AttributeValue.Create((uint)value);
    }

    public X509CertificateObject()
    {
        this.CkaSubject = Array.Empty<byte>();
        this.CkaId = Array.Empty<byte>();
        this.CkaIssuer = Array.Empty<byte>();
        this.CkaSerialNumber = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();

        this.CkaHashOfSubjectPublicKey = Array.Empty<byte>();
        this.CkaHashOfIssuerPublicKey = Array.Empty<byte>();
        this.CkaJavaMidpSecurityDomain = 0;

        this.CkaNameHashAlgorithm = CKM.CKM_SHA_1;
    }

    internal X509CertificateObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_SUBJECT, this.CkaSubject, true);
        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_ISSUER, this.CkaIssuer, true);
        CryptoObjectValueChecker.CheckDerInteger(CKA.CKA_SERIAL_NUMBER, this.CkaSerialNumber, true, true);


        if (this.CkaValue.Length == 0 && this.CkaUrl.Length == 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attributes CKA_VALUE or CKA_URL must contains valid value.");
        }

        CryptoObjectValueChecker.CheckX509DerCertificate(CKA.CKA_VALUE, this.CkaValue, true);


        this.CheckValueAndUrl();

        CryptoObjectValueChecker.CheckDigestValue(CKA.CKA_HASH_OF_SUBJECT_PUBLIC_KEY,
            this.CkaNameHashAlgorithm,
            this.CkaHashOfSubjectPublicKey,
            true);

        CryptoObjectValueChecker.CheckDigestValue(CKA.CKA_HASH_OF_ISSUER_PUBLIC_KEY,
            this.CkaNameHashAlgorithm,
            this.CkaHashOfIssuerPublicKey,
            true);

        //TODO: this.CkaHashOfSubjectPublicKey can by empty is URL is empty
    }

    private void CheckValueAndUrl()
    {
        if (this.CkaValue.Length == 0 && this.CkaUrl.Length == 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {CKA.CKA_VALUE} or {CKA.CKA_VALUE} must have a value.");
        }

        if (this.CkaValue.Length != 0 && this.CkaUrl.Length != 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Only one of the attributes {CKA.CKA_VALUE} and {CKA.CKA_VALUE} must have a value.");
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

    public override void ReComputeAttributes()
    {
        if (this.CkaSubject.Length > 0 && this.CkaHashOfSubjectPublicKey.Length == 0)
        {
            this.CkaHashOfSubjectPublicKey = DigestUtils.Compute(this.CkaNameHashAlgorithm, this.CkaSubject);
        }

        if (this.CkaIssuer.Length > 0 && this.CkaHashOfIssuerPublicKey.Length == 0)
        {
            this.CkaHashOfSubjectPublicKey = DigestUtils.Compute(this.CkaNameHashAlgorithm, this.CkaIssuer);
        }

        if (this.CkaValue.Length > 0 && this.CkaCheckValue.Length == 0)
        {
            this.CkaCheckValue = DigestUtils.ComputeCheckValue(this.CkaValue);
        }
    }
}
