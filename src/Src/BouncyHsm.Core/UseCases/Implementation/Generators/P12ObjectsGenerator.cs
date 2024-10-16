using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation.Generators;

internal class P12ObjectsGenerator
{
    private readonly OneKeyPkcs12 p12;
    private readonly string ckaLabel;
    private readonly byte[] ckaId;

    public PrivateKeyImportMode ImportMode
    {
        get;
        set;
    }

    public P12ObjectsGenerator(byte[] content,
        string? password,
        string ckaLabel,
        byte[] ckaId)
    {
        this.p12 = new OneKeyPkcs12(content, password);
        this.ckaLabel = ckaLabel;
        this.ckaId = ckaId;

        this.ImportMode = PrivateKeyImportMode.Imported;
    }

    public PrivateKeyObject CreatePrivateKey()
    {
        PrivateKeyObject privateKeyObject = this.p12.Certificate.KeyType switch
        {
            CKK.CKK_RSA => new RsaPrivateKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN),
            CKK.CKK_ECDSA => new EcdsaPrivateKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN),
            _ => throw new BouncyHsmInvalidInputException("Unsupported key type in P12 file.")
        };

        privateKeyObject.SetPrivateKey(this.p12.PrivateKey);
        privateKeyObject.CkaId = this.ckaId;
        privateKeyObject.CkaCopyable = false;
        privateKeyObject.CkaDestroyable = true;
        privateKeyObject.CkaLabel = this.ckaLabel;
        privateKeyObject.CkaModifiable = false;
        privateKeyObject.CkaPrivate = true;
        privateKeyObject.CkaToken = true;

        P11KeyUsages keyUsages = this.p12.Certificate.GetKeyUsage();
        privateKeyObject.CkaSign = keyUsages.CanSignAndVerify;
        privateKeyObject.CkaSignRecover = false;
        privateKeyObject.CkaDecrypt = keyUsages.CanEncryptAndDecrypt;
        privateKeyObject.CkaUnwrap = keyUsages.CanEncryptAndDecrypt;
        privateKeyObject.CkaDerive = keyUsages.CanDerive;
        this.UpdateAttributesByMode(privateKeyObject);

        privateKeyObject.ReComputeAttributes();

        return privateKeyObject;
    }

    public PublicKeyObject CreatePublicKey()
    {
        PublicKeyObject publicKeyObject = this.p12.Certificate.KeyType switch
        {
            CKK.CKK_RSA => new RsaPublicKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN),
            CKK.CKK_ECDSA => new EcdsaPublicKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN),
            _ => throw new BouncyHsmInvalidInputException("Unsupported key type in P12 file.")
        };

        publicKeyObject.SetPublicKey(this.p12.Certificate.ExtractPublicKey());
        publicKeyObject.CkaId = this.ckaId;
        publicKeyObject.CkaLabel = this.ckaLabel;
        publicKeyObject.CkaCopyable = false;
        publicKeyObject.CkaDestroyable = true;
        publicKeyObject.CkaModifiable = false;
        publicKeyObject.CkaPrivate = false;
        publicKeyObject.CkaToken = true;
        publicKeyObject.CkaTrusted = false;

        P11KeyUsages keyUsages = this.p12.Certificate.GetKeyUsage();
        publicKeyObject.CkaVerify = keyUsages.CanSignAndVerify;
        publicKeyObject.CkaVerifyRecover = keyUsages.CanSignAndVerify;
        publicKeyObject.CkaEncrypt = keyUsages.CanEncryptAndDecrypt;
        publicKeyObject.CkaWrap = keyUsages.CanEncryptAndDecrypt;
        publicKeyObject.CkaDerive = keyUsages.CanDerive;

        this.UpdateAttributesByMode(publicKeyObject);

        publicKeyObject.ReComputeAttributes();

        return publicKeyObject;
    }

    public X509CertificateObject CreateCertificate()
    {
        X509CertObjectGenerator generator = new X509CertObjectGenerator(this.p12.Certificate,
            this.ckaId,
            this.ckaLabel);

        return generator.CreateCertificateObject(false);
    }

    public X509CertificateObject[] GetCertificateChain()
    {
        X509CertificateWrapper[] chain = this.p12.CertificateChain;
        if (chain.Length == 0)
        {
            return Array.Empty<X509CertificateObject>();
        }

        List<X509CertificateObject> objects = new List<X509CertificateObject>(chain.Length);
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain[i].Certificate.Equals(this.p12.Certificate.Certificate))
            {
                continue;
            }

            X509CertObjectGenerator generator = new X509CertObjectGenerator(chain[i],
               this.ckaId,
               this.ckaLabel);

            objects.Add(generator.CreateCertificateObject(true));
        }

        return objects.ToArray();
    }

    private void UpdateAttributesByMode(PublicKeyObject publicKeyObject)
    {
        switch (this.ImportMode)
        {
            case PrivateKeyImportMode.Imported:
                publicKeyObject.CkaLocal = false;
                break;

            case PrivateKeyImportMode.Local:
                publicKeyObject.CkaLocal = true;
                break;

            case PrivateKeyImportMode.LocalInQualifiedArea:
                publicKeyObject.CkaLocal = true;
                break;

            default:
                throw new InvalidProgramException($"Enum value {this.ImportMode} is not supported.");
        }
    }

    private void UpdateAttributesByMode(PrivateKeyObject privateKeyObject)
    {
        switch (this.ImportMode)
        {
            case PrivateKeyImportMode.Imported:
                privateKeyObject.CkaLocal = false;
                privateKeyObject.CkaSensitive = false;
                privateKeyObject.CkaAlwaysSensitive = false;
                privateKeyObject.CkaAlwaysAuthenticate = false;
                privateKeyObject.CkaExtractable = true;
                privateKeyObject.CkaNewerExtractable = false;
                break;

            case PrivateKeyImportMode.Local:
                privateKeyObject.CkaLocal = true;
                privateKeyObject.CkaSensitive = true;
                privateKeyObject.CkaAlwaysSensitive = true;
                privateKeyObject.CkaAlwaysAuthenticate = false;
                privateKeyObject.CkaExtractable = false;
                privateKeyObject.CkaNewerExtractable = true;
                break;

            case PrivateKeyImportMode.LocalInQualifiedArea:
                privateKeyObject.CkaLocal = true;
                privateKeyObject.CkaSensitive = true;
                privateKeyObject.CkaAlwaysSensitive = true;
                privateKeyObject.CkaAlwaysAuthenticate = true;
                privateKeyObject.CkaExtractable = false;
                privateKeyObject.CkaNewerExtractable = true;
                break;

            default:
                throw new InvalidProgramException($"Enum value {this.ImportMode} is not supported.");
        }
    }
}
