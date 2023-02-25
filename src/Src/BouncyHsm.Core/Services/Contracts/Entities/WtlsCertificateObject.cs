using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Contracts.Entities;

/// <summary>
/// WTLS certificate object - special validation is not supported
/// </summary>
public sealed class WtlsCertificateObject : CertificateObject
{
    public override CKC CkaCertificateType
    {
        get => CKC.CKC_WTLS;
    }

    public byte[] CkaSubject
    {
        get => this.values[CKA.CKA_SUBJECT].AsByteArray();
        set => this.values[CKA.CKA_SUBJECT] = AttributeValue.Create(value);
    }

    public byte[] CkaIssuer
    {
        get => this.values[CKA.CKA_ISSUER].AsByteArray();
        set => this.values[CKA.CKA_ISSUER] = AttributeValue.Create(value);
    }

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

    public CKM CkaNameHashAlgorithm
    {
        get => (CKM)this.values[CKA.CKA_NAME_HASH_ALGORITHM].AsUint();
        set => this.values[CKA.CKA_NAME_HASH_ALGORITHM] = AttributeValue.Create((uint)value);
    }

    public WtlsCertificateObject()
    {
        this.CkaSubject = Array.Empty<byte>();
        this.CkaIssuer = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();
        this.CkaUrl = string.Empty;

        this.CkaHashOfSubjectPublicKey = Array.Empty<byte>();
        this.CkaHashOfIssuerPublicKey = Array.Empty<byte>();
        this.CkaNameHashAlgorithm = CKM.CKM_SHA_1;
    }

    internal WtlsCertificateObject(StorageObjectMemento memento)
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

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckDigestValue(CKA.CKA_HASH_OF_SUBJECT_PUBLIC_KEY,
            this.CkaNameHashAlgorithm,
            this.CkaHashOfSubjectPublicKey,
            true);

        CryptoObjectValueChecker.CheckDigestValue(CKA.CKA_HASH_OF_ISSUER_PUBLIC_KEY,
            this.CkaNameHashAlgorithm,
            this.CkaHashOfIssuerPublicKey,
            true);
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