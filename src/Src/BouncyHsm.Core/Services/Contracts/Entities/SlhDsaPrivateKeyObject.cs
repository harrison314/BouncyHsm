using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class SlhDsaPrivateKeyObject : PrivateKeyObject
{
    public CK_SLH_DSA_PARAMETER_SET CkaParameterSet
    {
        get => (CK_SLH_DSA_PARAMETER_SET)this.values[CKA.CKA_PARAMETER_SET].AsUint();
        set => this.values[CKA.CKA_PARAMETER_SET] = AttributeValue.Create((uint)value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public SlhDsaPrivateKeyObject() : base(CKK.CKK_SLH_DSA, CKM.CKM_SLH_DSA_KEY_PAIR_GEN)
    {
        this.CkaParameterSet = CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F;
        this.CkaValue = Array.Empty<byte>();
    }

    internal SlhDsaPrivateKeyObject(StorageObjectMemento memento)
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
        return SlhDsaPrivateKeyParameters.FromEncoding(SlhDsaUtils.GetParametersFromType(this.CkaParameterSet), this.CkaValue);
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is SlhDsaPrivateKeyParameters slhDsaPrivateKey)
        {
            this.CkaParameterSet = SlhDsaUtils.GetMlDsaparametersType(slhDsaPrivateKey.Parameters);

            this.CkaValue = slhDsaPrivateKey.GetEncoded();
        }
        else
        {
            throw new ArgumentException("publicKey is not SlhDsaPrivateKeyParameters", nameof(privateKey));
        }
    }

    public override void Validate()
    {
        base.Validate();
        CryptoObjectValueChecker.CheckEnumIsDefined<CK_SLH_DSA_PARAMETER_SET>(CKA.CKA_PARAMETER_SET, this.CkaParameterSet);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);
    }
}