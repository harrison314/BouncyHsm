using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Implementation.Visitors;

internal abstract class BaseObjectVisitor<T> : ICryptoApiObjectVisitor<T>
{
    public BaseObjectVisitor()
    {

    }

    public virtual T Visit(ClockObject clockObject)
    {
        return this.ProcessStorageObject(clockObject);
    }

    public virtual T Visit(DataObject dataObject)
    {
        return this.ProcessStorageObject(dataObject);
    }

    public virtual T Visit(RsaPrivateKeyObject rsaPrivateKeyObject)
    {
        return this.ProcessStorageObject(rsaPrivateKeyObject);
    }

    public virtual T Visit(RsaPublicKeyObject rsaPublicKeyObject)
    {
        return this.ProcessStorageObject(rsaPublicKeyObject);
    }

    public virtual T Visit(X509CertificateObject x509CertificateObject)
    {
        return this.ProcessStorageObject(x509CertificateObject);
    }

    public virtual T Visit(WtlsCertificateObject wtlsCertificateObject)
    {
        return this.ProcessStorageObject(wtlsCertificateObject);
    }

    public virtual T Visit(X509AttributeCertificateObject x509AttributeCertificateObject)
    {
        return this.ProcessStorageObject(x509AttributeCertificateObject);
    }

    public virtual T Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject)
    {
        return this.ProcessStorageObject(ecdsaPublicKeyObject);
    }

    public virtual T Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject)
    {
        return this.ProcessStorageObject(ecdsaPrivateKeyObject);
    }

    public virtual T Visit(GenericSecretKeyObject generalSecretKeyObject)
    {
        return this.ProcessStorageObject(generalSecretKeyObject);
    }

    public virtual T Visit(AesKeyObject aesKeyObject)
    {
        return this.ProcessStorageObject(aesKeyObject);
    }

    public virtual T Visit(Poly1305KeyObject poly1305KeyObject)
    {
        return this.ProcessStorageObject(poly1305KeyObject);
    }

    public virtual T Visit(ChaCha20KeyObject chaCha20KeyObject)
    {
        return this.ProcessStorageObject(chaCha20KeyObject);
    }

    public virtual T Visit(Salsa20KeyObject salsa20KeyObject)
    {
        return this.ProcessStorageObject(salsa20KeyObject);
    }

    public virtual T Visit(EdwardsPrivateKeyObject edwardsPrivateKey)
    {
        return this.ProcessStorageObject(edwardsPrivateKey);
    }

    public virtual T Visit(EdwardsPublicKeyObject edwardsPublicKey)
    {
        return this.ProcessStorageObject(edwardsPublicKey);
    }

    public virtual T Visit(MontgomeryPrivateKeyObject montgomeryPrivateKey)
    {
        return this.ProcessStorageObject(montgomeryPrivateKey);
    }

    public virtual T Visit(MontgomeryPublicKeyObject montgomeryPublicKey)
    {
        return this.ProcessStorageObject(montgomeryPublicKey);
    }

    public virtual T Visit(TrustObject trustObject)
    {
        return this.ProcessStorageObject(trustObject);
    }

    public virtual T Visit(MlDsaPublicKeyObject mlDsaPublicKeyObject)
    {
        return this.ProcessStorageObject(mlDsaPublicKeyObject);
    }

    public virtual T Visit(MlDsaPrivateKeyObject mlDsaPrivateKeyObject)
    {
        return this.ProcessStorageObject(mlDsaPrivateKeyObject);
    }

    public virtual T Visit(SlhDsaPublicKeyObject slhDsaPublicKeyObject)
    {
        return this.ProcessStorageObject(slhDsaPublicKeyObject);
    }

    public virtual T Visit(SlhDsaPrivateKeyObject slhDsaPrivateKeyObject)
    {
        return this.ProcessStorageObject(slhDsaPrivateKeyObject);
    }

    public virtual T Visit(MlKemPublicKeyObject mlKemPublicKeyObject)
    {
        return this.ProcessStorageObject(mlKemPublicKeyObject);
    }

    public virtual T Visit(MlKemPrivateKeyObject mlKemPrivateKeyObject)
    {
        return this.ProcessStorageObject(mlKemPrivateKeyObject);
    }

    public virtual T Visit(CamelliaKeyObject camelliaKeyObject)
    {
        return this.ProcessStorageObject(camelliaKeyObject);
    }

    protected virtual T ProcessStorageObject(ICryptoApiObject storageObject)
    {
        throw new NotSupportedException($"Object {storageObject.GetType().Name} is not supported.");
    }
}