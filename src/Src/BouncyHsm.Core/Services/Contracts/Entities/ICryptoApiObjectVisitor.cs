﻿namespace BouncyHsm.Core.Services.Contracts.Entities;

public interface ICryptoApiObjectVisitor
{
    void Visit(ClockObject clockObject);

    void Visit(DataObject dataObject);

    void Visit(RsaPrivateKeyObject rsaPrivateKeyObject);

    void Visit(RsaPublicKeyObject rsaPublicKeyObject);

    void Visit(X509CertificateObject x509CertificateObject);

    void Visit(WtlsCertificateObject wtlsCertificateObject);

    void Visit(X509AttributeCertificateObject x509AttributeCertificateObject);

    void Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject);

    void Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject);

    void Visit(GenericSecretKeyObject generalSecretKeyObject);

    void Visit(AesKeyObject aesKeyObject);

    void Visit(Poly1305KeyObject poly1305KeyObject);

    void Visit(ChaCha20KeyObject chaCha20KeyObject);

    void Visit(Salsa20KeyObject salsa20KeyObject);

    void Visit(EdwardsPrivateKeyObject edwardsPrivateKey);

    void Visit(EdwardsPublicKeyObject edwardsPublicKey);

    void Visit(MontgomeryPrivateKeyObject montgomeryPrivateKeyObject);

    void Visit(MontgomeryPublicKeyObject montgomeryPublicKeyObject);
}

public interface ICryptoApiObjectVisitor<out T>
{
    T Visit(ClockObject clockObject);

    T Visit(DataObject dataObject);

    T Visit(RsaPrivateKeyObject rsaPrivateKeyObject);

    T Visit(RsaPublicKeyObject rsaPublicKeyObject);

    T Visit(X509CertificateObject x509CertificateObject);

    T Visit(WtlsCertificateObject wtlsCertificateObject);

    T Visit(X509AttributeCertificateObject x509AttributeCertificateObject);

    T Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject);

    T Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject);

    T Visit(GenericSecretKeyObject generalSecretKeyObject);

    T Visit(AesKeyObject aesKeyObject);

    T Visit(Poly1305KeyObject poly1305KeyObject);

    T Visit(ChaCha20KeyObject chaCha20KeyObject);

    T Visit(Salsa20KeyObject salsa20KeyObject);

    T Visit(EdwardsPrivateKeyObject edwardsPrivateKey);

    T Visit(EdwardsPublicKeyObject edwardsPublicKey);

    T Visit(MontgomeryPrivateKeyObject montgomeryPrivateKeyObject);

    T Visit(MontgomeryPublicKeyObject montgomeryPublicKeyObject);
}