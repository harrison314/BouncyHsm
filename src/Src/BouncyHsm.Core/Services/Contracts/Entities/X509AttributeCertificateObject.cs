using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Org.BouncyCastle.Asn1;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class X509AttributeCertificateObject : CertificateObject
{
    public override CKC CkaCertificateType
    {
        get => CKC.CKC_X_509_ATTR_CERT;
    }

    public byte[] CkaOwner
    {
        get => this.values[CKA.CKA_OWNER].AsByteArray();
        set => this.values[CKA.CKA_OWNER] = AttributeValue.Create(value);
    }

    public byte[] CkaAcIssuer
    {
        get => this.values[CKA.CKA_AC_ISSUER].AsByteArray();
        set => this.values[CKA.CKA_AC_ISSUER] = AttributeValue.Create(value);
    }

    public byte[] CkaSerialNumber
    {
        get => this.values[CKA.CKA_SERIAL_NUMBER].AsByteArray();
        set => this.values[CKA.CKA_SERIAL_NUMBER] = AttributeValue.Create(value);
    }

    public byte[] CkaAttrTypes
    {
        get => this.values[CKA.CKA_ATTR_TYPES].AsByteArray();
        set => this.values[CKA.CKA_ATTR_TYPES] = AttributeValue.Create(value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public X509AttributeCertificateObject()
    {
        this.CkaOwner = Array.Empty<byte>();
        this.CkaAcIssuer = Array.Empty<byte>();
        this.CkaSerialNumber = Array.Empty<byte>();
        this.CkaAttrTypes = Array.Empty<byte>();
    }

    internal X509AttributeCertificateObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void Validate()
    {
        base.Validate();

        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_OWNER, this.CkaOwner, false);
        CryptoObjectValueChecker.CheckX509Name(CKA.CKA_AC_ISSUER, this.CkaAcIssuer, true);
        CryptoObjectValueChecker.CheckDerInteger(CKA.CKA_SERIAL_NUMBER, this.CkaSerialNumber, true, true);
        this.CheckAttributeTypes();
        CryptoObjectValueChecker.CheckX509DerCertificate(CKA.CKA_VALUE, this.CkaValue, false);
    }

    public List<DerObjectIdentifier> GetAttrTypes()
    {
        List<DerObjectIdentifier> types = new List<DerObjectIdentifier>();
        byte[] attrTypes = this.CkaAttrTypes;

        if (attrTypes.Length == 0)
        {
            return types;
        }

        Asn1Sequence sequence = (Asn1Sequence)Asn1Object.FromByteArray(attrTypes);
        for (int i = 0; i < sequence.Count; i++)
        {
            types.Add((DerObjectIdentifier)sequence[i]);
        }

        return types;
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    private void CheckAttributeTypes()
    {
        try
        {
            _ = this.GetAttrTypes();
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
               $"Attribute {CKA.CKA_ATTR_TYPES} is not valid sequence with oids DER encoding.",
               ex);
        }
    }

    public override void ReComputeAttributes()
    {
        if (this.CkaValue.Length > 0 && this.CkaCheckValue.Length == 0)
        {
            this.CkaCheckValue = DigestUtils.ComputeCheckValue(this.CkaValue);
        }
    }
}

