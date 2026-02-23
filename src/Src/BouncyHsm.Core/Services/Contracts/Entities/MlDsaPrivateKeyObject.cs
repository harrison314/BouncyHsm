using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography.X509Certificates;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class MlDsaPrivateKeyObject : PrivateKeyObject
{
    public CK_ML_DSA_PARAMETER_SET CkaParameterSet
    {
        get => (CK_ML_DSA_PARAMETER_SET)this.values[CKA.CKA_PARAMETER_SET].AsUint();
        set => this.values[CKA.CKA_PARAMETER_SET] = AttributeValue.Create((uint)value);
    }

    public byte[] CkaSeed
    {
        get => this.values[CKA.CKA_SEED].AsByteArray();
        set => this.values[CKA.CKA_SEED] = AttributeValue.Create(value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public MlDsaPrivateKeyObject() : base(CKK.CKK_ML_DSA, CKM.CKM_ML_DSA_KEY_PAIR_GEN)
    {
        this.CkaParameterSet = CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44;
        this.CkaSeed = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();
    }

    internal MlDsaPrivateKeyObject(StorageObjectMemento memento)
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
        return MLDsaPrivateKeyParameters.FromEncoding(MlDsaUtils.GetParametersFromType(this.CkaParameterSet), this.CkaValue);
    }

    public override void SetPrivateKey(AsymmetricKeyParameter privateKey)
    {
        if (privateKey is MLDsaPrivateKeyParameters mLDsaPrivateKey)
        {
            this.CkaParameterSet = MlDsaUtils.GetMlDsaparametersType(mLDsaPrivateKey.Parameters);

            // The seed should be filled in, but sometimes it is not possible to get it during import. This behavior is not in accordance with the specification.
            this.CkaSeed = mLDsaPrivateKey.GetSeed() ?? Array.Empty<byte>();
            this.CkaValue = mLDsaPrivateKey.GetEncoded();
        }
        else
        {
            throw new ArgumentException("publicKey is not MLDsaPrivateKeyParameters", nameof(privateKey));
        }
    }

    public override void Validate()
    {
        base.Validate();
        CryptoObjectValueChecker.CheckEnumIsDefined<CK_ML_DSA_PARAMETER_SET>(CKA.CKA_PARAMETER_SET, this.CkaParameterSet);

        // The seed should be filled in, but sometimes it is not possible to get it during import. This behavior is not in accordance with the specification.
        if (this.CkaValue.Length == 0)
        {
            CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_SEED, this.CkaSeed);
        }

        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);
    }
}