using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class TrustObject : StorageObject
{
    public override CKO CkaClass
    {
        get => CKO.CKO_TRUST;
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

    public byte[] CkaHashOfCertificate
    {
        get => this.values[CKA.CKA_HASH_OF_CERTIFICATE].AsByteArray();
        set => this.values[CKA.CKA_HASH_OF_CERTIFICATE] = AttributeValue.Create(value);
    }

    public CKM CkaMechanismType
    {
        get => (CKM)this.values[CKA.CKA_MECHANISM_TYPE].AsUint();
        set => this.values[CKA.CKA_MECHANISM_TYPE] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustServerAuth
    {
        get => (CKT)this.values[CKA.CKA_TRUST_SERVER_AUTH].AsUint();
        set => this.values[CKA.CKA_TRUST_SERVER_AUTH] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustClientAuth
    {
        get => (CKT)this.values[CKA.CKA_TRUST_CLIENT_AUTH].AsUint();
        set => this.values[CKA.CKA_TRUST_CLIENT_AUTH] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustCodeSigning
    {
        get => (CKT)this.values[CKA.CKA_TRUST_CODE_SIGNING].AsUint();
        set => this.values[CKA.CKA_TRUST_CODE_SIGNING] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustEmailProtection
    {
        get => (CKT)this.values[CKA.CKA_TRUST_EMAIL_PROTECTION].AsUint();
        set => this.values[CKA.CKA_TRUST_EMAIL_PROTECTION] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustIpsecIke
    {
        get => (CKT)this.values[CKA.CKA_TRUST_IPSEC_IKE].AsUint();
        set => this.values[CKA.CKA_TRUST_IPSEC_IKE] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustTimeStamping
    {
        get => (CKT)this.values[CKA.CKA_TRUST_TIME_STAMPING].AsUint();
        set => this.values[CKA.CKA_TRUST_TIME_STAMPING] = AttributeValue.Create((uint)value);
    }

    public CKT CkaTrustOcpsSigning
    {
        get => (CKT)this.values[CKA.CKA_TRUST_OCSP_SIGNING].AsUint();
        set => this.values[CKA.CKA_TRUST_OCSP_SIGNING] = AttributeValue.Create((uint)value);
    }

    public TrustObject()
    {
        this.CkaIssuer = Array.Empty<byte>();
        this.CkaSerialNumber = Array.Empty<byte>();
        this.CkaHashOfCertificate = Array.Empty<byte>();
        this.CkaMechanismType = CKM.CKM_SHA_1;
        this.CkaTrustServerAuth = CKT.CKT_TRUST_UNKNOWN;
        this.CkaTrustClientAuth = CKT.CKT_TRUST_UNKNOWN;
        this.CkaTrustCodeSigning = CKT.CKT_TRUST_UNKNOWN;
        this.CkaTrustEmailProtection = CKT.CKT_TRUST_UNKNOWN;
        this.CkaTrustIpsecIke = CKT.CKT_TRUST_UNKNOWN;
        this.CkaTrustTimeStamping = CKT.CKT_TRUST_UNKNOWN;
        this.CkaTrustOcpsSigning = CKT.CKT_TRUST_UNKNOWN;
    }

    internal TrustObject(StorageObjectMemento memento)
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

    public override void ReComputeAttributes()
    {

    }

    public override void Validate()
    {
        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_ISSUER, this.CkaIssuer, false);
        CryptoObjectValueChecker.CheckDerInteger(CKA.CKA_SERIAL_NUMBER, this.CkaSerialNumber, false, true);

        if (this.GetTrustValues().All(v => v is CKT.CKT_TRUST_UNKNOWN or CKT.CKT_NOT_TRUSTED))
        {
            CryptoObjectValueChecker.CheckDigestValue(CKA.CKA_HASH_OF_CERTIFICATE,
               this.CkaMechanismType,
               this.CkaHashOfCertificate,
               false);
        }
        else
        {
            CryptoObjectValueChecker.CheckDigestValue(CKA.CKA_HASH_OF_CERTIFICATE,
               this.CkaMechanismType,
               this.CkaHashOfCertificate,
               true);
        }

        this.CheckTrustValue(this.CkaTrustServerAuth, CKA.CKA_TRUST_SERVER_AUTH);
        this.CheckTrustValue(this.CkaTrustClientAuth, CKA.CKA_TRUST_CLIENT_AUTH);
        this.CheckTrustValue(this.CkaTrustCodeSigning, CKA.CKA_TRUST_CODE_SIGNING);
        this.CheckTrustValue(this.CkaTrustEmailProtection, CKA.CKA_TRUST_EMAIL_PROTECTION);
        this.CheckTrustValue(this.CkaTrustIpsecIke, CKA.CKA_TRUST_IPSEC_IKE);
        this.CheckTrustValue(this.CkaTrustTimeStamping, CKA.CKA_TRUST_TIME_STAMPING);
        this.CheckTrustValue(this.CkaTrustOcpsSigning, CKA.CKA_TRUST_OCSP_SIGNING);
    }

    private void CheckTrustValue(CKT value, CKA attributeName)
    {
        if (!Enum.IsDefined<CKT>(value))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"Attribute {attributeName} has invalid trust value {value}.");
        }
    }

    private IEnumerable<CKT> GetTrustValues()
    {
        yield return this.CkaTrustServerAuth;
        yield return this.CkaTrustClientAuth;
        yield return this.CkaTrustCodeSigning;
        yield return this.CkaTrustEmailProtection;
        yield return this.CkaTrustIpsecIke;
        yield return this.CkaTrustTimeStamping;
        yield return this.CkaTrustOcpsSigning;
    }
}
