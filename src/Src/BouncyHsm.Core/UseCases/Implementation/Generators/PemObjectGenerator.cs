using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation.Generators;

internal class PemObjectGenerator
{
    private readonly byte[] ckaId;
    private readonly string ckaLabel;

    public bool ForSigning
    {
        get;
        set;
    }

    public bool ForEncryption
    {
        get;
        set;
    }

    public bool ForDerivation
    {
        get;
        set;
    }

    public bool ForWrap
    {
        get;
        set;
    }

    public PrivateKeyImportMode ImportMode
    {
        get;
        set;
    }

    public PemObjectGenerator(byte[] ckaId,
        string ckaLabel)
    {
        this.ckaId = ckaId;
        this.ckaLabel = ckaLabel;
    }

    public IReadOnlyList<StorageObject> GenerateObjects(string pem, char[]? password)
    {
        List<StorageObject> objects = new List<StorageObject>();

        foreach (object cryptoObject in this.ReadPem(pem, password))
        {
            object internalObject;
            if(cryptoObject is AsymmetricCipherKeyPair keyPair)
            {
                internalObject = keyPair.Private ?? keyPair.Public;
            }
            else
            {
                internalObject = cryptoObject;
            }

            StorageObject storageObject = internalObject switch
            {
                ECPrivateKeyParameters key => this.CreateEcPrivateKey(key),
                Ed25519PrivateKeyParameters key => this.CreateEd25519PrivateKey(key),
                Ed448PrivateKeyParameters key => this.CreateEd448PrivateKey(key),
                ECPublicKeyParameters key => this.CreateEcPublicKey(key),
                Ed25519PublicKeyParameters key => this.CreateEd25519PublicKey(key),
                Ed448PublicKeyParameters key => this.CreateEd448PublicKey(key),
                RsaPrivateCrtKeyParameters key => this.CreateRsaPrivateKey(key),
                RsaKeyParameters key => this.CreateRsaPublicKey(key),
                X509Certificate certificate => this.CreateCertificate(certificate),
                Org.BouncyCastle.Utilities.IO.Pem.PemObject pemObject => this.CreateFromPem(pemObject),
                _ => throw new IOException("PEM object not supported.")
            };

            objects.Add(storageObject);
        }

        return objects;
    }

    private StorageObject CreateFromPem(Org.BouncyCastle.Utilities.IO.Pem.PemObject pemObject)
    {
        return pemObject.Type switch
        {
            "GENERIC SECRET" => this.CreateGenericSecret(pemObject.Content, pemObject.Headers),
            "AES SECRET KEY" => this.CreateAesKey(pemObject.Content),
            "POLY1305 SECRET KEY" => this.CreatePoly1305Key(pemObject.Content),
            "CHACHA20 SECRET KEY" => this.CreateChaCha20Key(pemObject.Content),
            "SALSA20 SECRET KEY" => this.CreateSalsa20Key(pemObject.Content),
            "DATA OBJECT" => this.CreateDataObject(pemObject.Content, pemObject.Headers),
            _ => throw new IOException($"PEM object not supported ({pemObject.Type}).")
        };
    }

    private StorageObject CreateGenericSecret(byte[] content, IList<Org.BouncyCastle.Utilities.IO.Pem.PemHeader> headers)
    {
        CKK keyType = CKK.CKK_GENERIC_SECRET;

        Org.BouncyCastle.Utilities.IO.Pem.PemHeader? keyTypeHeader = headers.SingleOrDefault(t => string.Equals(t.Name, "KeyType"));
        if (keyTypeHeader != null)
        {
            if (Enum.TryParse<CKK>(keyTypeHeader.Value.Trim(), out CKK keyTypeResult))
            {
                keyType = keyTypeResult;
            }
            else
            {
                throw new IOException($"KeyType in GENERIC SECRET has invalid value.");
            }
        }

        if (keyType == CKK.CKK_AES)
        {
            return this.CreateAesKey(content);
        }

        GenericSecretKeyObject genericSecretKeyObject = new GenericSecretKeyObject();
        genericSecretKeyObject.CkaKeyType = keyType;
        genericSecretKeyObject.CkaCopyable = false;
        genericSecretKeyObject.CkaDecrypt = this.ForEncryption;
        genericSecretKeyObject.CkaDerive = this.ForDerivation;
        genericSecretKeyObject.CkaDestroyable = true;
        genericSecretKeyObject.CkaEncrypt = this.ForEncryption;
        genericSecretKeyObject.CkaId = this.ckaId;
        genericSecretKeyObject.CkaLabel = this.ckaLabel;
        genericSecretKeyObject.CkaModifiable = false;
        genericSecretKeyObject.CkaPrivate = true;
        genericSecretKeyObject.CkaSign = this.ForSigning;
        genericSecretKeyObject.CkaToken = true;
        genericSecretKeyObject.CkaTrusted = true;
        genericSecretKeyObject.CkaUnwrap = this.ForWrap;

        genericSecretKeyObject.SetSecret(content);

        this.UpdateAttributesByMode(genericSecretKeyObject);

        genericSecretKeyObject.ReComputeAttributes();

        return genericSecretKeyObject;
    }

    private StorageObject CreateAesKey(byte[] content)
    {
        AesKeyObject aesKeyObject = new AesKeyObject();
        aesKeyObject.CkaCopyable = false;
        aesKeyObject.CkaDecrypt = this.ForEncryption;
        aesKeyObject.CkaDerive = this.ForDerivation;
        aesKeyObject.CkaDestroyable = true;
        aesKeyObject.CkaEncrypt = this.ForEncryption;
        aesKeyObject.CkaId = this.ckaId;
        aesKeyObject.CkaLabel = this.ckaLabel;
        aesKeyObject.CkaModifiable = false;
        aesKeyObject.CkaPrivate = true;
        aesKeyObject.CkaSign = this.ForSigning;
        aesKeyObject.CkaToken = true;
        aesKeyObject.CkaTrusted = true;
        aesKeyObject.CkaUnwrap = this.ForWrap;

        aesKeyObject.SetSecret(content);

        this.UpdateAttributesByMode(aesKeyObject);

        aesKeyObject.ReComputeAttributes();

        return aesKeyObject;
    }

    private StorageObject CreatePoly1305Key(byte[] content)
    {
        Poly1305KeyObject keyObject = new Poly1305KeyObject();
        keyObject.CkaCopyable = false;
        keyObject.CkaDecrypt = this.ForEncryption;
        keyObject.CkaDerive = this.ForDerivation;
        keyObject.CkaDestroyable = true;
        keyObject.CkaEncrypt = this.ForEncryption;
        keyObject.CkaId = this.ckaId;
        keyObject.CkaLabel = this.ckaLabel;
        keyObject.CkaModifiable = false;
        keyObject.CkaPrivate = true;
        keyObject.CkaSign = this.ForSigning;
        keyObject.CkaToken = true;
        keyObject.CkaTrusted = true;
        keyObject.CkaUnwrap = this.ForWrap;

        keyObject.SetSecret(content);

        this.UpdateAttributesByMode(keyObject);

        keyObject.ReComputeAttributes();

        return keyObject;
    }

    private StorageObject CreateChaCha20Key(byte[] content)
    {
        ChaCha20KeyObject keyObject = new ChaCha20KeyObject();
        keyObject.CkaCopyable = false;
        keyObject.CkaDecrypt = this.ForEncryption;
        keyObject.CkaDerive = this.ForDerivation;
        keyObject.CkaDestroyable = true;
        keyObject.CkaEncrypt = this.ForEncryption;
        keyObject.CkaId = this.ckaId;
        keyObject.CkaLabel = this.ckaLabel;
        keyObject.CkaModifiable = false;
        keyObject.CkaPrivate = true;
        keyObject.CkaSign = this.ForSigning;
        keyObject.CkaToken = true;
        keyObject.CkaTrusted = true;
        keyObject.CkaUnwrap = this.ForWrap;

        keyObject.SetSecret(content);

        this.UpdateAttributesByMode(keyObject);

        keyObject.ReComputeAttributes();

        return keyObject;
    }

    private StorageObject CreateSalsa20Key(byte[] content)
    {
        Salsa20KeyObject keyObject = new Salsa20KeyObject();
        keyObject.CkaCopyable = false;
        keyObject.CkaDecrypt = this.ForEncryption;
        keyObject.CkaDerive = this.ForDerivation;
        keyObject.CkaDestroyable = true;
        keyObject.CkaEncrypt = this.ForEncryption;
        keyObject.CkaId = this.ckaId;
        keyObject.CkaLabel = this.ckaLabel;
        keyObject.CkaModifiable = false;
        keyObject.CkaPrivate = true;
        keyObject.CkaSign = this.ForSigning;
        keyObject.CkaToken = true;
        keyObject.CkaTrusted = true;
        keyObject.CkaUnwrap = this.ForWrap;

        keyObject.SetSecret(content);

        this.UpdateAttributesByMode(keyObject);

        keyObject.ReComputeAttributes();

        return keyObject;
    }

    private StorageObject CreateDataObject(byte[] content, IList<Org.BouncyCastle.Utilities.IO.Pem.PemHeader> headers)
    {
        DataObject dataObject = new DataObject();
        dataObject.CkaCopyable = false;
        dataObject.CkaDestroyable = true;
        dataObject.CkaLabel = this.ckaLabel;
        dataObject.CkaModifiable = false;
        dataObject.CkaValue = content;
        dataObject.CkaPrivate = true;
        dataObject.CkaToken = true;

        Org.BouncyCastle.Utilities.IO.Pem.PemHeader? applicationHeader = headers.SingleOrDefault(t => string.Equals(t.Name, "Application"));
        if (applicationHeader != null)
        {
            dataObject.CkaApplication = applicationHeader.Value.Trim();
        }

        Org.BouncyCastle.Utilities.IO.Pem.PemHeader? objectIdHeader = headers.SingleOrDefault(t => string.Equals(t.Name, "ObjectId"));
        if (objectIdHeader != null)
        {
            dataObject.CkaObjectId = new Org.BouncyCastle.Asn1.DerObjectIdentifier(objectIdHeader.Value.Trim()).GetEncoded();
        }

        return dataObject;
    }

    private StorageObject CreateCertificate(X509Certificate certificate)
    {
        X509CertificateWrapper certificateWrapper = X509CertificateWrapper.FromInstance(certificate);
        X509CertObjectGenerator generator = new X509CertObjectGenerator(certificateWrapper,
            this.ckaId,
            this.ckaLabel);

        return generator.CreateCertificateObject(false);
    }

    private StorageObject CreateEcPublicKey(ECPublicKeyParameters key)
    {
        EcdsaPublicKeyObject publicKeyObject = new EcdsaPublicKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN);
        publicKeyObject.SetPublicKey(key);
        publicKeyObject.CkaId = this.ckaId;
        publicKeyObject.CkaLabel = this.ckaLabel;
        publicKeyObject.CkaCopyable = false;
        publicKeyObject.CkaDestroyable = true;
        publicKeyObject.CkaModifiable = false;
        publicKeyObject.CkaPrivate = false;
        publicKeyObject.CkaToken = true;
        publicKeyObject.CkaTrusted = false;

        publicKeyObject.CkaVerify = this.ForSigning;
        publicKeyObject.CkaVerifyRecover = false;
        publicKeyObject.CkaEncrypt = this.ForEncryption;
        publicKeyObject.CkaWrap = this.ForWrap;
        publicKeyObject.CkaDerive = this.ForDerivation;

        this.UpdateAttributesByMode(publicKeyObject);

        publicKeyObject.ReComputeAttributes();

        return publicKeyObject;
    }

    private StorageObject CreateEd25519PublicKey(Ed25519PublicKeyParameters key)
    {
        EdwardsPublicKeyObject publicKeyObject = new EdwardsPublicKeyObject(CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN);
        publicKeyObject.SetPublicKey(key);
        publicKeyObject.CkaId = this.ckaId;
        publicKeyObject.CkaLabel = this.ckaLabel;
        publicKeyObject.CkaCopyable = false;
        publicKeyObject.CkaDestroyable = true;
        publicKeyObject.CkaModifiable = false;
        publicKeyObject.CkaPrivate = false;
        publicKeyObject.CkaToken = true;
        publicKeyObject.CkaTrusted = false;

        publicKeyObject.CkaVerify = this.ForSigning;
        publicKeyObject.CkaVerifyRecover = false;
        publicKeyObject.CkaEncrypt = this.ForEncryption;
        publicKeyObject.CkaWrap = this.ForWrap;
        publicKeyObject.CkaDerive = this.ForDerivation;

        this.UpdateAttributesByMode(publicKeyObject);

        publicKeyObject.ReComputeAttributes();

        return publicKeyObject;
    }

    private StorageObject CreateEd448PublicKey(Ed448PublicKeyParameters key)
    {
        EdwardsPublicKeyObject publicKeyObject = new EdwardsPublicKeyObject(CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN);
        publicKeyObject.SetPublicKey(key);
        publicKeyObject.CkaId = this.ckaId;
        publicKeyObject.CkaLabel = this.ckaLabel;
        publicKeyObject.CkaCopyable = false;
        publicKeyObject.CkaDestroyable = true;
        publicKeyObject.CkaModifiable = false;
        publicKeyObject.CkaPrivate = false;
        publicKeyObject.CkaToken = true;
        publicKeyObject.CkaTrusted = false;

        publicKeyObject.CkaVerify = this.ForSigning;
        publicKeyObject.CkaVerifyRecover = false;
        publicKeyObject.CkaEncrypt = this.ForEncryption;
        publicKeyObject.CkaWrap = this.ForWrap;
        publicKeyObject.CkaDerive = this.ForDerivation;

        this.UpdateAttributesByMode(publicKeyObject);

        publicKeyObject.ReComputeAttributes();

        return publicKeyObject;
    }

    private StorageObject CreateEcPrivateKey(ECPrivateKeyParameters key)
    {
        EcdsaPrivateKeyObject privateKeyObject = new EcdsaPrivateKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        privateKeyObject.SetPrivateKey(key);
        privateKeyObject.CkaId = this.ckaId;
        privateKeyObject.CkaCopyable = false;
        privateKeyObject.CkaDestroyable = true;
        privateKeyObject.CkaLabel = this.ckaLabel;
        privateKeyObject.CkaModifiable = false;
        privateKeyObject.CkaPrivate = true;
        privateKeyObject.CkaToken = true;

        privateKeyObject.CkaSign = this.ForSigning;
        privateKeyObject.CkaSignRecover = false;
        privateKeyObject.CkaDecrypt = this.ForEncryption;
        privateKeyObject.CkaUnwrap = this.ForWrap;
        privateKeyObject.CkaDerive = this.ForDerivation;
        this.UpdateAttributesByMode(privateKeyObject);

        privateKeyObject.ReComputeAttributes();

        return privateKeyObject;
    }

    private StorageObject CreateEd25519PrivateKey(Ed25519PrivateKeyParameters key)
    {
        EdwardsPrivateKeyObject privateKeyObject = new EdwardsPrivateKeyObject(CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN);

        privateKeyObject.SetPrivateKey(key);
        privateKeyObject.CkaId = this.ckaId;
        privateKeyObject.CkaCopyable = false;
        privateKeyObject.CkaDestroyable = true;
        privateKeyObject.CkaLabel = this.ckaLabel;
        privateKeyObject.CkaModifiable = false;
        privateKeyObject.CkaPrivate = true;
        privateKeyObject.CkaToken = true;

        privateKeyObject.CkaSign = this.ForSigning;
        privateKeyObject.CkaSignRecover = false;
        privateKeyObject.CkaDecrypt = this.ForEncryption;
        privateKeyObject.CkaUnwrap = this.ForWrap;
        privateKeyObject.CkaDerive = this.ForDerivation;
        this.UpdateAttributesByMode(privateKeyObject);

        privateKeyObject.ReComputeAttributes();

        return privateKeyObject;
    }

    private StorageObject CreateEd448PrivateKey(Ed448PrivateKeyParameters key)
    {
        EdwardsPrivateKeyObject privateKeyObject = new EdwardsPrivateKeyObject(CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN);

        privateKeyObject.SetPrivateKey(key);
        privateKeyObject.CkaId = this.ckaId;
        privateKeyObject.CkaCopyable = false;
        privateKeyObject.CkaDestroyable = true;
        privateKeyObject.CkaLabel = this.ckaLabel;
        privateKeyObject.CkaModifiable = false;
        privateKeyObject.CkaPrivate = true;
        privateKeyObject.CkaToken = true;

        privateKeyObject.CkaSign = this.ForSigning;
        privateKeyObject.CkaSignRecover = false;
        privateKeyObject.CkaDecrypt = this.ForEncryption;
        privateKeyObject.CkaUnwrap = this.ForWrap;
        privateKeyObject.CkaDerive = this.ForDerivation;
        this.UpdateAttributesByMode(privateKeyObject);

        privateKeyObject.ReComputeAttributes();

        return privateKeyObject;
    }

    private StorageObject CreateRsaPublicKey(RsaKeyParameters key)
    {
        RsaPublicKeyObject publicKeyObject = new RsaPublicKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);
        publicKeyObject.SetPublicKey(key);
        publicKeyObject.CkaId = this.ckaId;
        publicKeyObject.CkaLabel = this.ckaLabel;
        publicKeyObject.CkaCopyable = false;
        publicKeyObject.CkaDestroyable = true;
        publicKeyObject.CkaModifiable = false;
        publicKeyObject.CkaPrivate = false;
        publicKeyObject.CkaToken = true;
        publicKeyObject.CkaTrusted = false;

        publicKeyObject.CkaVerify = this.ForSigning;
        publicKeyObject.CkaVerifyRecover = false;
        publicKeyObject.CkaEncrypt = this.ForEncryption;
        publicKeyObject.CkaWrap = this.ForWrap;
        publicKeyObject.CkaDerive = this.ForDerivation;

        this.UpdateAttributesByMode(publicKeyObject);

        publicKeyObject.ReComputeAttributes();

        return publicKeyObject;
    }

    private StorageObject CreateRsaPrivateKey(RsaPrivateCrtKeyParameters key)
    {
        RsaPrivateKeyObject privateKeyObject = new RsaPrivateKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);

        privateKeyObject.SetPrivateKey(key);
        privateKeyObject.CkaId = this.ckaId;
        privateKeyObject.CkaCopyable = false;
        privateKeyObject.CkaDestroyable = true;
        privateKeyObject.CkaLabel = this.ckaLabel;
        privateKeyObject.CkaModifiable = false;
        privateKeyObject.CkaPrivate = true;
        privateKeyObject.CkaToken = true;

        privateKeyObject.CkaSign = this.ForSigning;
        privateKeyObject.CkaSignRecover = false;
        privateKeyObject.CkaDecrypt = this.ForEncryption;
        privateKeyObject.CkaUnwrap = this.ForWrap;
        privateKeyObject.CkaDerive = this.ForDerivation;
        this.UpdateAttributesByMode(privateKeyObject);

        privateKeyObject.ReComputeAttributes();

        return privateKeyObject;
    }

    private List<object> ReadPem(string pem, char[]? password)
    {
        PemPasswordFinder pemPasswordFinder = new PemPasswordFinder(password);

        using StringReader stringReader = new StringReader(pem);
        using StringReader alternativeStringReader = new StringReader(pem);
        PemReader pemReader = new PemReader(stringReader, pemPasswordFinder);
        PemReader alternativePemReader = new PemReader(alternativeStringReader, pemPasswordFinder);

        List<object> result = new List<object>();
        object item;
        for (; ; )
        {
            try
            {
                item = pemReader.ReadObject();
                _ = alternativePemReader.ReadPemObject();
            }
            catch (IOException)
            {
                item = alternativePemReader.ReadPemObject();
            }

            if (item == null)
            {
                break;
            }

            result.Add(item);
        }

        return result;
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

    private void UpdateAttributesByMode(SecretKeyObject keyObject)
    {
        switch (this.ImportMode)
        {
            case PrivateKeyImportMode.Imported:
                keyObject.CkaLocal = false;
                keyObject.CkaSensitive = false;
                keyObject.CkaAlwaysSensitive = false;
                keyObject.CkaExtractable = true;
                keyObject.CkaNewerExtractable = false;
                break;

            case PrivateKeyImportMode.Local:
                keyObject.CkaLocal = true;
                keyObject.CkaSensitive = true;
                keyObject.CkaAlwaysSensitive = true;
                keyObject.CkaExtractable = false;
                keyObject.CkaNewerExtractable = true;
                break;

            case PrivateKeyImportMode.LocalInQualifiedArea:
                keyObject.CkaLocal = true;
                keyObject.CkaSensitive = true;
                keyObject.CkaAlwaysSensitive = true;
                keyObject.CkaExtractable = false;
                keyObject.CkaNewerExtractable = true;
                break;

            default:
                throw new InvalidProgramException($"Enum value {this.ImportMode} is not supported.");
        }
    }
}
