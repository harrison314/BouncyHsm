using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;

namespace BouncyHsm.Core.Services.Bc;

public class X509CertificateWrapper
{
    private readonly X509Certificate certificate;

    public static X509CertificateWrapper FromInstance(byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        X509CertificateParser parser = new X509CertificateParser();
        X509Certificate certificate = parser.ReadCertificate(data);

        return new X509CertificateWrapper(certificate);
    }

    public static X509CertificateWrapper FromInstance(X509Certificate certificate)
    {
        System.Diagnostics.Debug.Assert(certificate != null);

        return new X509CertificateWrapper(certificate);
    }

    public CKK KeyType
    {
        get;
    }

    public X509Certificate Certificate
    {
        get => this.certificate;
    }

    public CkDate CkaStartDate
    {
        get => new CkDate(this.certificate.NotBefore.ToUniversalTime());
    }

    public CkDate CkaEndDate
    {
        get => new CkDate(this.certificate.NotAfter.ToUniversalTime());
    }

    public byte[] CkaIssuer
    {
        get => this.certificate.IssuerDN.GetEncoded();
    }

    public byte[] CkaSubject
    {
        get => this.certificate.SubjectDN.GetEncoded();
    }

    public byte[] CkaSerialNumber
    {
        get => new DerInteger(this.certificate.SerialNumber).GetEncoded();
    }

    public byte[] CkaValue
    {
        get => this.certificate.GetEncoded();
    }

    private X509CertificateWrapper(X509Certificate certificate)
    {
        this.certificate = certificate;
        this.KeyType = this.GetKeyType();
    }

    public P11KeyUsages GetKeyUsage()
    {
        bool[] keyUsage = this.certificate.GetKeyUsage();

        bool canSign = keyUsage[KeyUsageBitsIndex.CRLSign]
            || keyUsage[KeyUsageBitsIndex.DigitalSignature]
            || keyUsage[KeyUsageBitsIndex.KeyCertSign]
            || keyUsage[KeyUsageBitsIndex.NonRepudiation];

        bool canEncrypt = keyUsage[KeyUsageBitsIndex.EncipherOnly]
            || keyUsage[KeyUsageBitsIndex.DecipherOnly]
            || keyUsage[KeyUsageBitsIndex.DataEncipherment];

        bool canDerive = keyUsage[KeyUsageBitsIndex.KeyAgreement];

        return new P11KeyUsages(canSign, canEncrypt, canDerive);
    }

    public AsymmetricKeyParameter ExtractPublicKey()
    {
        return this.certificate.GetPublicKey();
    }

    private CKK GetKeyType()
    {
        AsymmetricKeyParameter publicKey = this.certificate.GetPublicKey();

        return publicKey switch
        {
            RsaKeyParameters _ => CKK.CKK_RSA,
            ECPublicKeyParameters _ => CKK.CKK_ECDSA,
            DHPublicKeyParameters => CKK.CKK_DH,
            DsaPublicKeyParameters _ => CKK.CKK_DSA,
            _ => throw new NotSupportedException($"Not supported public key in certificate {publicKey.GetType().Name}.")
        };
    }
}
