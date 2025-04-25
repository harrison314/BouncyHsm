using BouncyHsm.Core.Services.Contracts.P11;
using System;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public abstract class SecretKeyObject : KeyObject
{
    public override CKO CkaClass
    {
        get => CKO.CKO_SECRET_KEY;
    }

    public bool CkaSensitive
    {
        get => this.values[CKA.CKA_SENSITIVE].AsBool();
        set => this.values[CKA.CKA_SENSITIVE] = AttributeValue.Create(value);
    }

    public bool CkaEncrypt
    {
        get => this.values[CKA.CKA_ENCRYPT].AsBool();
        set => this.values[CKA.CKA_ENCRYPT] = AttributeValue.Create(value);
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

    public bool CkaVerify
    {
        get => this.values[CKA.CKA_VERIFY].AsBool();
        set => this.values[CKA.CKA_VERIFY] = AttributeValue.Create(value);
    }

    public bool CkaWrap
    {
        get => this.values[CKA.CKA_WRAP].AsBool();
        set => this.values[CKA.CKA_WRAP] = AttributeValue.Create(value);
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

    public byte[] CkaCheckValue
    {
        get => this.values[CKA.CKA_CHECK_VALUE].AsByteArray();
        set => this.values[CKA.CKA_CHECK_VALUE] = AttributeValue.Create(value);
    }

    public bool CkaWrapWithTrusted
    {
        get => this.values[CKA.CKA_WRAP_WITH_TRUSTED].AsBool();
        set => this.values[CKA.CKA_WRAP_WITH_TRUSTED] = AttributeValue.Create(value);
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

    //TODO: Implement uint array attribute
    //public byte[] CkaUnwrapTemplate
    //{
    //    get => this.values[CKA.CKA_UNWRAP_TEMPLATE].AsByteArray();
    //    set => this.values[CKA.CKA_UNWRAP_TEMPLATE] = AttributeValue.Create(value);
    //}

    protected SecretKeyObject(CKK keyType, CKM genMechanism)
        : base(keyType, genMechanism)
    {
        this.CkaSensitive = false;
        this.CkaEncrypt = false;
        this.CkaDecrypt = false;
        this.CkaSign = false;
        this.CkaVerify = false;
        this.CkaWrap = false;
        this.CkaUnwrap = false;
        this.CkaExtractable = false;
        this.CkaAlwaysSensitive = false;
        this.CkaNewerExtractable = false;
        this.CkaCheckValue = Array.Empty<byte>();
        this.CkaWrapWithTrusted = false;
        this.CkaTrusted = false;
    }

    protected SecretKeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void ReComputeAttributes()
    {
        this.CkaAlwaysSensitive = this.CkaAlwaysSensitive && this.CkaSensitive;
        this.CkaNewerExtractable = this.CkaNewerExtractable && this.CkaExtractable;

        // this.CkaCheckValue is not implemeted - is ingored
        // https://docs.oasis-open.org/pkcs11/pkcs11-base/v2.40/os/pkcs11-base-v2.40-os.html
        // Section 4.10
        // Is firt 3 bytes of reslut of encryption with CKA_VALUE as key and data block with all zeros bytes.
        // CKA_CHECK_VALUE <= SymetricEncrypt(cipher: AES_CBC, key: CKA_VALUE, data: 0x0000...).Slice(0, 3)
    }

    public abstract byte[] GetSecret();

    public abstract void SetSecret(byte[] secret);

    public abstract uint GetMinimalSecretLen();

    public abstract uint? GetRequiredSecretLen();

    internal double GetKeyEntropy()
    {
        Span<uint> histogram = stackalloc uint[256];
        histogram.Fill(0U);

        byte[] key = this.GetSecret();

        for (int i = 0; i < key.Length; i++)
        {
            histogram[key[i]]++;
        }

        double entropy = 0;

        for (int i = 0; i < histogram.Length; i++)
        {
            if (histogram[i] != 0)
            {
                double probability = (double)histogram[i] / key.LongLength;
                entropy -= probability * Math.Log(probability, 2.0);
            }
        }

        return entropy;
    }
}
