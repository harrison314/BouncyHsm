using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation.Visitors;

internal class ObjectContentVisitor : ICryptoApiObjectVisitor<DomainResult<ObjectContent>>
{
    public ObjectContentVisitor()
    {

    }

    public DomainResult<ObjectContent> Visit(ClockObject clockObject)
    {
        return new DomainResult<ObjectContent>.InvalidInput("Clock object is not downloadable.");
    }

    public DomainResult<ObjectContent> Visit(DataObject dataObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("data.bin", dataObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(RsaPrivateKeyObject rsaPrivateKeyObject)
    {
        return this.CreatePemResult("rsa_private_key.pem", rsaPrivateKeyObject.GetPrivateKey());
    }

    public DomainResult<ObjectContent> Visit(RsaPublicKeyObject rsaPublicKeyObject)
    {
        return this.CreatePemResult("rsa_public_key.pem", SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(rsaPublicKeyObject.GetPublicKey()));
    }

    public DomainResult<ObjectContent> Visit(X509CertificateObject x509CertificateObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("certificate.cer", x509CertificateObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(WtlsCertificateObject wtlsCertificateObject)
    {
        return new DomainResult<ObjectContent>.InvalidInput("Wtls certificate object is not downloadable.");
    }

    public DomainResult<ObjectContent> Visit(X509AttributeCertificateObject x509AttributeCertificateObject)
    {
        return new DomainResult<ObjectContent>.InvalidInput("X509 attribute certificate object is not downloadable.");
    }

    public DomainResult<ObjectContent> Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject)
    {
        return this.CreatePemResult("rsa_public_key.pem", SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(ecdsaPublicKeyObject.GetPublicKey()));
    }

    public DomainResult<ObjectContent> Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject)
    {
        return this.CreatePemResult("ec_private_key.pem", ecdsaPrivateKeyObject.GetPrivateKey());
    }

    public DomainResult<ObjectContent> Visit(GenericSecretKeyObject generalSecretKeyObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("secret.bin", generalSecretKeyObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(AesKeyObject aesKeyObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("aes_key.bin", aesKeyObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(Poly1305KeyObject poly1305KeyObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("poly1305_key.bin", poly1305KeyObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(ChaCha20KeyObject chaCha20KeyObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("chacha20_key.bin", chaCha20KeyObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(Salsa20KeyObject salsa20KeyObject)
    {
        return new DomainResult<ObjectContent>.Ok(new ObjectContent("salsa20_key.bin", salsa20KeyObject.CkaValue));
    }

    public DomainResult<ObjectContent> Visit(EdwardsPrivateKeyObject edwardsPrivateKey)
    {
        return this.CreatePemResult("ec_ed_private_key.pem", edwardsPrivateKey.GetPrivateKey());

    }

    public DomainResult<ObjectContent> Visit(EdwardsPublicKeyObject edwardsPublicKey)
    {
        return this.CreatePemResult("ec_ed_public_key.pem", edwardsPublicKey.GetPublicKey());
    }

    public DomainResult<ObjectContent> Visit(MontgomeryPrivateKeyObject montgomeryPrivateKey)
    {
        return this.CreatePemResult("ec_x_private_key.pem", montgomeryPrivateKey.GetPrivateKey());
    }

    public DomainResult<ObjectContent> Visit(MontgomeryPublicKeyObject montgomeryPublicKey)
    {
        return this.CreatePemResult("ec_x_public_key.pem", montgomeryPublicKey.GetPublicKey());
    }

    private DomainResult<ObjectContent> CreatePemResult(string fileName, object pemObject)
    {
        using MemoryStream ms = new MemoryStream();
        using StreamWriter sw = new StreamWriter(ms);
        PemWriter pemWriter = new PemWriter(sw);
        pemWriter.WriteObject(pemObject);
        sw.Flush();

        return new DomainResult<ObjectContent>.Ok(new ObjectContent(fileName, ms.ToArray()));
    }
}
