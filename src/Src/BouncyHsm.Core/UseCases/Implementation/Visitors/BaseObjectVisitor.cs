﻿using BouncyHsm.Core.Services.Contracts.Entities;

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

    public T Visit(AesKeyObject aesKeyObject)
    {
        return this.ProcessStorageObject(aesKeyObject);
    }

    public T Visit(Poly1305KeyObject poly1305KeyObject)
    {
        return this.ProcessStorageObject(poly1305KeyObject);
    }

    public T Visit(ChaCha20KeyObject chaCha20KeyObject)
    {
        return this.ProcessStorageObject(chaCha20KeyObject);
    }

    public T Visit(Salsa20KeyObject salsa20KeyObject)
    {
        return this.ProcessStorageObject(salsa20KeyObject);
    }

    public T Visit(EdwardsPrivateKeyObject edwardsPrivateKey)
    {
        return this.ProcessStorageObject(edwardsPrivateKey);
    }

    public T Visit(EdwardsPublicKeyObject edwardsPublicKey)
    {
        return this.ProcessStorageObject(edwardsPublicKey);
    }

    public T Visit(MontgomeryPrivateKeyObject montgomeryPrivateKey)
    {
        return this.ProcessStorageObject(montgomeryPrivateKey);
    }

    public T Visit(MontgomeryPublicKeyObject montgomeryPublicKey)
    {
        return this.ProcessStorageObject(montgomeryPublicKey);
    }

    protected virtual T ProcessStorageObject(ICryptoApiObject storageObject)
    {
        throw new NotSupportedException($"Object {storageObject.GetType().Name} is not supported.");
    }
}