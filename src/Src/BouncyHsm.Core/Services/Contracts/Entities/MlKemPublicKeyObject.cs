using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class MlKemPublicKeyObject : PublicKeyObject
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

    public MlKemPublicKeyObject()
        : base(CKK.CKK_ML_KEM, CKM.CKM_ML_KEM_KEY_PAIR_GEN)
    {
        this.CkaParameterSet = CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512;
        this.CkaValue = Array.Empty<byte>();
    }

    internal MlKemPublicKeyObject(StorageObjectMemento memento)
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

    public override AsymmetricKeyParameter GetPublicKey()
    {
        return MLKemPublicKeyParameters.FromEncoding(MlKemUtils.GetParametersFromType(this.CkaParameterSet), this.CkaValue);
    }

    public override void SetPublicKey(AsymmetricKeyParameter publicKey)
    {
        if (publicKey is MLKemPublicKeyParameters mlKemPublicKey)
        {
            this.CkaParameterSet = MlKemUtils.GetMlDsaparametersType(mlKemPublicKey.Parameters);
            this.CkaValue = mlKemPublicKey.GetEncoded();
        }
        else
        {
            throw new ArgumentException("publicKey is not MLKemPublicKeyParameters", nameof(publicKey));
        }
    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckEnumIsDefined<CK_ML_KEM_PARAMETER_SET>(CKA.CKA_PARAMETER_SET, this.CkaParameterSet);
        CryptoObjectValueChecker.CheckNotEmpty(CKA.CKA_VALUE, this.CkaValue);
    }
}
