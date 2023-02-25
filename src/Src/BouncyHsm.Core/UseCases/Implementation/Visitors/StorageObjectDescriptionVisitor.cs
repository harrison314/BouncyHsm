using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Utils;
using BouncyHsm.Core.UseCases.Contracts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation.Visitors;

internal class StorageObjectDescriptionVisitor : ICryptoApiObjectVisitor<string>
{
    public StorageObjectDescriptionVisitor()
    {

    }

    public string Visit(ClockObject clockObject)
    {
        throw new NotSupportedException();
    }

    public string Visit(DataObject dataObject)
    {
        return "DataObject";
    }

    public string Visit(RsaPrivateKeyObject rsaPrivateKeyObject)
    {
        uint keySize = (uint)rsaPrivateKeyObject.CkaModulus.Length*8;

        return $"Private key RSA-{keySize}";
    }

    public string Visit(RsaPublicKeyObject rsaPublicKeyObject)
    {
        return $"Public key RSA-{rsaPublicKeyObject.CkaModulusBits}";
    }

    public string Visit(X509CertificateObject x509CertificateObject)
    {
        return "X509 Certificate";
    }

    public string Visit(WtlsCertificateObject wtlsCertificateObject)
    {
        return "WTLS Certificate";
    }

    public string Visit(X509AttributeCertificateObject x509AttributeCertificateObject)
    {
        return "WTLS Attribute Certificate";
    }

    public string Visit(EcdsaPublicKeyObject ecdsaPublicKeyObject)
    {
        return $"Public key ECDSA - {EcdsaUtils.ParseEcParamsAsName(ecdsaPublicKeyObject.CkaEcParams)}";
    }

    public string Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject)
    {
        return $"Private key ECDSA - {EcdsaUtils.ParseEcParamsAsName(ecdsaPrivateKeyObject.CkaEcParams)}";
    }

    public string Visit(GenericSecretKeyObject generalSecretKeyObject)
    {
        return $"Secret Key ({generalSecretKeyObject.GetSecret().Length}B)";
    }
}
