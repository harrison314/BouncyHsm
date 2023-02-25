using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public abstract class CertificateObject : StorageObject
{
    public override CKO CkaClass
    {
        get => CKO.CKO_CERTIFICATE;
    }

    public abstract CKC CkaCertificateType
    {
        get;
    }

    // Trusted certificates cannot be modified.
    public bool CkaTrusted
    {
        get => this.values[CKA.CKA_TRUSTED].AsBool();
        set => this.values[CKA.CKA_TRUSTED] = AttributeValue.Create(value);
    }

    public CKCertificateCategory CkaCertificateCategory
    {
        get => (CKCertificateCategory)this.values[CKA.CKA_CERTIFICATE_CATEGORY].AsUint();
        set => this.values[CKA.CKA_CERTIFICATE_CATEGORY] = AttributeValue.Create((uint)value);
    }

    public byte[] CkaCheckValue
    {
        get => this.values[CKA.CKA_CHECK_VALUE].AsByteArray();
        set => this.values[CKA.CKA_CHECK_VALUE] = AttributeValue.Create(value);
    }

    public CkDate CkaStartDate
    {
        get => this.values[CKA.CKA_START_DATE].AsDate();
        set => this.values[CKA.CKA_START_DATE] = AttributeValue.Create(value);
    }

    public CkDate CkaEndDate
    {
        get => this.values[CKA.CKA_END_DATE].AsDate();
        set => this.values[CKA.CKA_END_DATE] = AttributeValue.Create(value);
    }

    public byte[] CkaPublicKeyInfo
    {
        get => this.values[CKA.CKA_PUBLIC_KEY_INFO].AsByteArray();
        set => this.values[CKA.CKA_PUBLIC_KEY_INFO] = AttributeValue.Create(value);
    }

    public CertificateObject()
    {
        this.values[CKA.CKA_CERTIFICATE_TYPE] = AttributeValue.Create((uint)this.CkaCertificateType);
        this.CkaTrusted = false;
        this.CkaCertificateCategory = CKCertificateCategory.CK_CERTIFICATE_CATEGORY_UNSPECIFIED;
        this.CkaCheckValue = Array.Empty<byte>();

        this.CkaStartDate = new CkDate();
        this.CkaEndDate = new CkDate();

        this.CkaPublicKeyInfo = Array.Empty<byte>();
    }

    internal CertificateObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    //public override void SetValue(CKA attributeType, IAttributeValue value)
    //{
    //    if (attributeType == CKA.CKA_TRUSTED)
    //    {
    //        throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_READ_ONLY, "Attribute CKA_TRUSTED is readonly.");
    //    }

    //    base.SetValue(attributeType, value);
    //}

    public override void Validate()
    {
        if (!Enum.IsDefined<CKCertificateCategory>(this.CkaCertificateCategory))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "Value of CKA_CERTIFICATE_CATEGORY is invalid.");
        }

        CryptoObjectValueChecker.CheckIsCheckValue(CKA.CKA_CHECK_VALUE, this.CkaCheckValue);
        CryptoObjectValueChecker.CheckPublicSubjectKeyInfo(CKA.CKA_PUBLIC_KEY_INFO, this.CkaPublicKeyInfo, true);
        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_PUBLIC_KEY_INFO, this.CkaPublicKeyInfo, true);
        CryptoObjectValueChecker.CheckStartEndDate(this.CkaStartDate, this.CkaEndDate);
    }
}
