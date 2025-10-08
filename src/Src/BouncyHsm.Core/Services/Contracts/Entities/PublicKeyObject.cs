using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public abstract class PublicKeyObject : KeyObject
{
    public override CKO CkaClass
    {
        get => CKO.CKO_PUBLIC_KEY;
    }

    public byte[] CkaSubject
    {
        get => this.values[CKA.CKA_SUBJECT].AsByteArray();
        set => this.values[CKA.CKA_SUBJECT] = AttributeValue.Create(value);
    }

    public bool CkaEncrypt
    {
        get => this.values[CKA.CKA_ENCRYPT].AsBool();
        set => this.values[CKA.CKA_ENCRYPT] = AttributeValue.Create(value);
    }

    public bool CkaVerify
    {
        get => this.values[CKA.CKA_VERIFY].AsBool();
        set => this.values[CKA.CKA_VERIFY] = AttributeValue.Create(value);
    }

    public bool CkaVerifyRecover
    {
        get => this.values[CKA.CKA_VERIFY_RECOVER].AsBool();
        set => this.values[CKA.CKA_VERIFY_RECOVER] = AttributeValue.Create(value);
    }

    public bool CkaWrap
    {
        get => this.values[CKA.CKA_WRAP].AsBool();
        set => this.values[CKA.CKA_WRAP] = AttributeValue.Create(value);
    }

    public bool CkaTrusted
    {
        get => this.values[CKA.CKA_TRUSTED].AsBool();
        set => this.values[CKA.CKA_TRUSTED] = AttributeValue.Create(value);
    }

    //TODO: list of attribute array
    //public byte[] CkaWrapTemplate
    //{
    //    get => this.values[CKA.CKA_WRAP_TEMPLATE].AsByteArray();
    //    set => this.values[CKA.CKA_WRAP_TEMPLATE] = AttributeValue.Create(value);
    //}

    public byte[] CkaPublicKeyInfo
    {
        get => this.values[CKA.CKA_PUBLIC_KEY_INFO].AsByteArray();
        set => this.values[CKA.CKA_PUBLIC_KEY_INFO] = AttributeValue.Create(value);
    }

    protected PublicKeyObject(CKK keyType, CKM genMechanism)
        : base(keyType, genMechanism)
    {
        this.CkaSubject = Array.Empty<byte>();
        this.CkaEncrypt = false;
        this.CkaVerify = false;
        this.CkaVerifyRecover = false;
        this.CkaWrap = false;
        this.CkaTrusted = false;
        this.CkaPublicKeyInfo = Array.Empty<byte>();
    }

    internal PublicKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_SUBJECT, this.CkaSubject, true);
        CryptoObjectValueChecker.CheckPublicSubjectKeyInfo(CKA.CKA_PUBLIC_KEY_INFO, this.CkaPublicKeyInfo, true);
    }

    public abstract Org.BouncyCastle.Crypto.AsymmetricKeyParameter GetPublicKey();

    public abstract void SetPublicKey(Org.BouncyCastle.Crypto.AsymmetricKeyParameter publicKey);

    public virtual SubjectPublicKeyInfo GetSubjectPublicKeyInfo()
    {
        if (this.CkaPublicKeyInfo.Length > 0)
        {
            return SubjectPublicKeyInfo.GetInstance(this.CkaPublicKeyInfo);

        }

        return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(this.GetPublicKey());
    }

    public override void ReComputeAttributes()
    {
        // NOP
    }
}
