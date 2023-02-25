using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public abstract class PrivateKeyObject : KeyObject
{
    public override CKO CkaClass
    {
        get => CKO.CKO_PRIVATE_KEY;
    }

    public byte[] CkaSubject
    {
        get => this.values[CKA.CKA_SUBJECT].AsByteArray();
        set => this.values[CKA.CKA_SUBJECT] = AttributeValue.Create(value);
    }

    public bool CkaSensitive
    {
        get => this.values[CKA.CKA_SENSITIVE].AsBool();
        set => this.values[CKA.CKA_SENSITIVE] = AttributeValue.Create(value);
    }

    public bool CkaDecrypt
    {
        get => this.values[CKA.CKA_DECRYPT].AsBool();
        set => this.values[CKA.CKA_DECRYPT] = AttributeValue.Create(value);
    }

    public bool CkaSign
    {
        get => this.values[CKA.CKA_SIGN].AsBool();
        set => this.values[CKA.CKA_SIGN] = AttributeValue.Create(value);
    }

    public bool CkaSignRecover
    {
        get => this.values[CKA.CKA_SIGN_RECOVER].AsBool();
        set => this.values[CKA.CKA_SIGN_RECOVER] = AttributeValue.Create(value);
    }

    public bool CkaUnwrap
    {
        get => this.values[CKA.CKA_UNWRAP].AsBool();
        set => this.values[CKA.CKA_UNWRAP] = AttributeValue.Create(value);
    }

    public bool CkaExtractable
    {
        get => this.values[CKA.CKA_EXTRACTABLE].AsBool();
        set => this.values[CKA.CKA_EXTRACTABLE] = AttributeValue.Create(value);
    }

    public bool CkaAlwaysSensitive
    {
        get => this.values[CKA.CKA_ALWAYS_SENSITIVE].AsBool();
        set => this.values[CKA.CKA_ALWAYS_SENSITIVE] = AttributeValue.Create(value);
    }

    public bool CkaNewerExtractable
    {
        get => this.values[CKA.CKA_NEVER_EXTRACTABLE].AsBool();
        set => this.values[CKA.CKA_NEVER_EXTRACTABLE] = AttributeValue.Create(value);
    }

    public bool CkaWrapWithTrusted
    {
        get => this.values[CKA.CKA_WRAP_WITH_TRUSTED].AsBool();
        set => this.values[CKA.CKA_WRAP_WITH_TRUSTED] = AttributeValue.Create(value);
    }

    //TODO: list of attribute array
    //public byte[] CkaUnwrapTemplate
    //{
    //    get => this.values[CKA.CKA_UNWRAP_TEMPLATE].AsByteArray();
    //    set => this.values[CKA.CKA_UNWRAP_TEMPLATE] = AttributeValue.Create(value);
    //}

    public bool CkaAlwaysAuthenticate
    {
        get => this.values[CKA.CKA_ALWAYS_AUTHENTICATE].AsBool();
        set => this.values[CKA.CKA_ALWAYS_AUTHENTICATE] = AttributeValue.Create(value);
    }

    protected PrivateKeyObject(CKK keyType, CKM genMechanism)
      : base(keyType, genMechanism)
    {
        this.CkaSubject = Array.Empty<byte>();
        this.CkaSensitive = false;
        this.CkaDecrypt = false;
        this.CkaSign = false;
        this.CkaSignRecover = false;
        this.CkaUnwrap = false;
        this.CkaExtractable = false;
        this.CkaAlwaysSensitive = false;
        this.CkaNewerExtractable = false;
        this.CkaWrapWithTrusted = false;
        this.CkaAlwaysAuthenticate = false;
    }

    internal PrivateKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_SUBJECT, this.CkaSubject, true);
    }

    public abstract Org.BouncyCastle.Crypto.AsymmetricKeyParameter GetPrivateKey();

    public abstract void SetPrivateKey(Org.BouncyCastle.Crypto.AsymmetricKeyParameter privateKey);

    public override void ReComputeAttributes()
    {
        this.CkaAlwaysSensitive = this.CkaAlwaysSensitive && this.CkaSensitive;
        this.CkaNewerExtractable = this.CkaNewerExtractable && this.CkaExtractable;
    }
}
