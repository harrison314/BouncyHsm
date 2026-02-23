using BouncyHsm.Core.Services.Contracts.Entities;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal abstract class BaseKeyTimeVisitor : ICryptoApiObjectVisitor<TimeSpan>
{
    public BaseKeyTimeVisitor()
    {

    }

    public TimeSpan Visit(ClockObject clockObject)
    {
        this.NotSupported(clockObject);
        return default;
    }

    public TimeSpan Visit(DataObject dataObject)
    {
        this.NotSupported(dataObject);
        return default;
    }

    public abstract TimeSpan Visit(RsaPrivateKeyObject rsaPrivateKeyObject);

    public TimeSpan Visit(RsaPublicKeyObject rsaPublicKeyObject)
    {
        this.NotSupported(rsaPublicKeyObject);
        return default;
    }

    public TimeSpan Visit(X509CertificateObject x509CertificateObject)
    {
        this.NotSupported(x509CertificateObject);
        return default;
    }

    public TimeSpan Visit(WtlsCertificateObject wtlsCertificateObject)
    {
        this.NotSupported(wtlsCertificateObject);
        return default;
    }

    public TimeSpan Visit(X509AttributeCertificateObject x509AttributeCertificateObject)
    {
        this.NotSupported(x509AttributeCertificateObject);
        return default;
    }

    public TimeSpan Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject)
    {
        this.NotSupported(ecdsaPublicKeyObject);
        return default;
    }

    public abstract TimeSpan Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject);

    public abstract TimeSpan Visit(GenericSecretKeyObject generalSecretKeyObject);

    public abstract TimeSpan Visit(AesKeyObject aesKeyObject);

    public abstract TimeSpan Visit(Poly1305KeyObject poly1305KeyObject);

    public abstract TimeSpan Visit(ChaCha20KeyObject chaCha20KeyObject);

    public abstract TimeSpan Visit(Salsa20KeyObject salsa20KeyObject);

    public abstract TimeSpan Visit(EdwardsPrivateKeyObject edwardsPrivateKey);

    public TimeSpan Visit(EdwardsPublicKeyObject edwardsPublicKey)
    {
        this.NotSupported(edwardsPublicKey);
        return default;
    }

    public abstract TimeSpan Visit(MontgomeryPrivateKeyObject montgomeryPrivateKey);

    public TimeSpan Visit(MontgomeryPublicKeyObject montgomeryPublicKey)
    {
        this.NotSupported(montgomeryPublicKey);
        return default;
    }

    public TimeSpan Visit(TrustObject trustObject)
    {
        this.NotSupported(trustObject);
        return default;
    }

    public TimeSpan Visit(MlDsaPublicKeyObject mlDsaPublicKeyObject)
    {
        this.NotSupported(mlDsaPublicKeyObject);
        return default;
    }

    public abstract TimeSpan Visit(MlDsaPrivateKeyObject mlDsaPrivateKeyObject);

    public TimeSpan Visit(SlhDsaPublicKeyObject slhDsaPublicKeyObject)
    {
        this.NotSupported(slhDsaPublicKeyObject);
        return default;
    }

    public abstract TimeSpan Visit(SlhDsaPrivateKeyObject slhDsaPrivateKeyObject);

    public TimeSpan Visit(MlKemPublicKeyObject mlKemPublicKeyObject)
    {
        this.NotSupported(mlKemPublicKeyObject);
        return default;
    }

    public abstract TimeSpan Visit(MlKemPrivateKeyObject mlKemPrivateKeyObject);

    public TimeSpan Visit(CamelliaKeyObject camelliaKeyObject)
    {
        this.NotSupported(camelliaKeyObject);
        return default;
    }

    [DoesNotReturn]
    protected virtual void NotSupported(ICryptoApiObject cryptoApiObject)
    {
        throw new NotSupportedException($"Object of type {cryptoApiObject.GetType().Name} is not supported.");
    }
}