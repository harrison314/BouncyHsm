using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class MlKemPrivateKeyObject : PrivateKeyObject
{
    public CK_ML_KEM_PARAMETER_SET CkaParameterSet
    {
        get => (CK_ML_KEM_PARAMETER_SET)this.values[CKA.CKA_PARAMETER_SET].AsUint();
        set => this.values[CKA.CKA_PARAMETER_SET] = AttributeValue.Create((uint)value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public MlKemPrivateKeyObject()
        : base(CKK.CKK_ML_KEM, CKM.CKM_ML_KEM_KEY_PAIR_GEN)
    {
        this.CkaParameterSet = CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512;
        this.CkaValue = Array.Empty<byte>();
    }

    internal MlKemPrivateKeyObject(StorageObjectMemento memento)
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
        return MLKemPrivateKeyParameters.FromEncoding(MlKemUtils.GetParametersFromType(this.CkaParameterSet), this.CkaValue);
    }

    public override void SetPrivateKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is MLKemPrivateKeyParameters mlKemPrivateKey)
        {
            this.CkaParameterSet = MlKemUtils.GetMlDsaparametersType(mlKemPrivateKey.Parameters);
            this.CkaValue = mlKemPrivateKey.GetEncoded();
        }
        else
        {
            throw new ArgumentException("publicKey is not MLKemPrivateKeyParameters", nameof(publicKey));
        }
    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckEnumIsDefined<CK_ML_KEM_PARAMETER_SET>(CKA.CKA_PARAMETER_SET, this.CkaParameterSet);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);
    }
}