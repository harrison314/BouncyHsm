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
        this.NotSuported(clockObject);
        return default;
    }

    public TimeSpan Visit(DataObject dataObject)
    {
        this.NotSuported(dataObject);
        return default;
    }

    public abstract TimeSpan Visit(RsaPrivateKeyObject rsaPrivateKeyObject);

    public TimeSpan Visit(RsaPublicKeyObject rsaPublicKeyObject)
    {
        this.NotSuported(rsaPublicKeyObject);
        return default;
    }

    public TimeSpan Visit(X509CertificateObject x509CertificateObject)
    {
        this.NotSuported(x509CertificateObject);
        return default;
    }

    public TimeSpan Visit(WtlsCertificateObject wtlsCertificateObject)
    {
        this.NotSuported(wtlsCertificateObject);
        return default;
    }

    public TimeSpan Visit(X509AttributeCertificateObject x509AttributeCertificateObject)
    {
        this.NotSuported(x509AttributeCertificateObject);
        return default;
    }

    public TimeSpan Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject)
    {
        this.NotSuported(ecdsaPublicKeyObject);
        return default;
    }

    public abstract TimeSpan Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject);

    public abstract TimeSpan Visit(GenericSecretKeyObject generalSecretKeyObject);

    public abstract TimeSpan Visit(AesKeyObject aesKeyObject);

    [DoesNotReturn]
    protected virtual void NotSuported(ICryptoApiObject cryptoApiObject)
    {
        throw new NotSupportedException($"Object of type {cryptoApiObject.GetType().Name} is not supported.");
    }
}