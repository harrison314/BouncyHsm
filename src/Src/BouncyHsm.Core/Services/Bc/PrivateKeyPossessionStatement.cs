using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BouncyHsm.Core.Services.Bc;

internal class PrivateKeyPossessionStatement : Asn1Encodable
{
    public static readonly DerObjectIdentifier id_at_statementOfPossession = new DerObjectIdentifier("1.3.6.1.4.1.22112.2.1");
    
    public IssuerAndSerialNumber Signer
    {
        get;
        protected set;
    }

    public X509CertificateStructure? Cert
    {
        get;
        protected set;
    }

    [return: NotNullIfNotNull(nameof(obj))]
    public static PrivateKeyPossessionStatement? GetInstance(object? obj)
    {
        if (obj == null) return null;

        return new PrivateKeyPossessionStatement(Asn1Sequence.GetInstance(obj));
    }

    public PrivateKeyPossessionStatement(IssuerAndSerialNumber signer, X509CertificateStructure? cert)
    {
        this.Signer = signer;
        this.Cert = cert;
    }

    public PrivateKeyPossessionStatement(X509Certificate certificate, bool includeCertificate)
    {
        this.Signer = new IssuerAndSerialNumber(certificate.CertificateStructure);
        this.Cert = includeCertificate ? certificate.CertificateStructure : null;
    }

    public PrivateKeyPossessionStatement(X509CertificateStructure certificate, bool includeCertificate)
    {
        this.Signer = new IssuerAndSerialNumber(certificate);
        this.Cert = includeCertificate ? certificate : null;
    }

    public PrivateKeyPossessionStatement(Asn1Sequence sequence)
    {
        if (sequence.Count == 1)
        {
            this.Signer = IssuerAndSerialNumber.GetInstance(sequence[0]);
            this.Cert = null;
        }
        else if (sequence.Count == 2)
        {
            this.Signer = IssuerAndSerialNumber.GetInstance(sequence[0]);
            this.Cert = X509CertificateStructure.GetInstance(sequence[0]);
        }
        else
        {
            throw new ArgumentException("invalid values in sequence");
        }
    }

    public override Asn1Object ToAsn1Object()
    {
        if (this.Cert == null)
        {
            return DerSequence.FromElement(this.Signer);
        }
        else
        {
            return DerSequence.FromElements(this.Signer, this.Cert);
        }
    }
}
