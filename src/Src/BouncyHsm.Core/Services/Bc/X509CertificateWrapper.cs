using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Net.Http.Headers;

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
        if (keyUsage == null)
        {
            P11KeyUsages? usage = this.TryGetKeyUsageFromExtendedKeyUsage();
            if (usage != null)
            {
                return usage;
            }

            return new P11KeyUsages(true, true, true);
        }

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

    public bool CheckPrivateKey(AsymmetricKeyParameter privateKey)
    {
        System.Diagnostics.Debug.Assert(privateKey != null);

        ISigner signer = this.KeyType switch
        {
            CKK.CKK_RSA => SignerUtilities.GetSigner("SHA1withRSA"),
            CKK.CKK_ECDSA => new DsaDigestSigner(new ECDsaSigner(), new Sha1Digest(), PlainDsaEncoding.Instance),
            CKK.CKK_EC_EDWARDS => this.CreateEdwardsSigner(privateKey),
            _ => throw new InvalidProgramException($"Enuum value {this.KeyType} not supported.")
        };

        byte[] data = new byte[20];
        Random.Shared.NextBytes(data);

        signer.Reset();
        signer.Init(true, privateKey);
        signer.BlockUpdate(data);
        byte[] signature = signer.GenerateSignature();

        signer.Reset();
        signer.Init(false, this.certificate.GetPublicKey());
        signer.BlockUpdate(data);

        return signer.VerifySignature(signature);
    }

    private ISigner CreateEdwardsSigner(AsymmetricKeyParameter privateKey)
    {
        return privateKey switch
        {
            Ed25519PrivateKeyParameters _ => new Ed25519Signer(),
            Ed448PrivateKeyParameters _ => new Ed448Signer(Array.Empty<byte>()),
            _ => throw new NotSupportedException($"Not supported private key in certificate {privateKey.GetType().Name}.")
        };
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
            Ed25519PublicKeyParameters _ => CKK.CKK_EC_EDWARDS,
            Ed448PublicKeyParameters _ => CKK.CKK_EC_EDWARDS,
            _ => throw new NotSupportedException($"Not supported public key in certificate {publicKey.GetType().Name}.")
        };
    }

    private P11KeyUsages? TryGetKeyUsageFromExtendedKeyUsage()
    {
        bool canSign = false;
        bool canEncrypt = false;

        IList<DerObjectIdentifier> usageOids = this.certificate.GetExtendedKeyUsage();
        if (usageOids == null)
        {
            return null;
        }

        foreach (DerObjectIdentifier usageOid in usageOids)
        {
            if (usageOid.Equals(Org.BouncyCastle.Asn1.X509.KeyPurposeID.id_kp_serverAuth)
                || usageOid.Equals(Org.BouncyCastle.Asn1.X509.KeyPurposeID.id_kp_clientAuth)
                || usageOid.Equals(Org.BouncyCastle.Asn1.X509.KeyPurposeID.id_kp_emailProtection)
                || usageOid.Id == "1.3.6.1.5.5.7.3.17")
            {
                canSign = true;
                canEncrypt = true;
            }
            else if (usageOid.Equals(Org.BouncyCastle.Asn1.X509.KeyPurposeID.id_kp_codeSigning)
                || usageOid.Equals(Org.BouncyCastle.Asn1.X509.KeyPurposeID.id_kp_timeStamping)
                || usageOid.Equals(Org.BouncyCastle.Asn1.X509.KeyPurposeID.id_kp_OCSPSigning))
            {
                canSign = true;
            }
        }

        if (canSign || canEncrypt)
        {
            return new P11KeyUsages(canSign, canEncrypt, false);
        }

        return null;
    }
}
