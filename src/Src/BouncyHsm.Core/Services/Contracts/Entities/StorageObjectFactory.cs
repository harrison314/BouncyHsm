using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public static class StorageObjectFactory
{
    #region Factory section
    internal enum StorageObjectInternalType
    {
        DataObject,
        RsaPrivateKeyObject,
        RsaPublicKeyObject,
        X509CertificateObject,
        WtlsCertificateObject,
        X509AttributeCertificateObject,
        EcdsaPublicKeyObject,
        EcdsaPrivateKeyObject,
        GenericSecretKeyObject,
        AesKeyObject
    }

    internal interface IStorageObjectInternalFactory
    {
        StorageObject Create(StorageObjectInternalType storageObjectType);
    }

    internal class NewStorageObjectInternalFactory : IStorageObjectInternalFactory
    {
        public NewStorageObjectInternalFactory()
        {

        }

        public StorageObject Create(StorageObjectInternalType storageObjectType)
        {
            return storageObjectType switch
            {
                StorageObjectInternalType.DataObject => new DataObject(),
                StorageObjectInternalType.RsaPrivateKeyObject => new RsaPrivateKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN),
                StorageObjectInternalType.RsaPublicKeyObject => new RsaPublicKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN),
                StorageObjectInternalType.X509CertificateObject => new X509CertificateObject(),
                StorageObjectInternalType.WtlsCertificateObject => new WtlsCertificateObject(),
                StorageObjectInternalType.X509AttributeCertificateObject => new X509AttributeCertificateObject(),
                StorageObjectInternalType.EcdsaPublicKeyObject => new EcdsaPublicKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN),
                StorageObjectInternalType.EcdsaPrivateKeyObject => new EcdsaPrivateKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN),
                StorageObjectInternalType.GenericSecretKeyObject => new GenericSecretKeyObject(),
                StorageObjectInternalType.AesKeyObject => new AesKeyObject(),
                _ => throw new InvalidProgramException($"Enum value {storageObjectType} is not supported.")
            };
        }
    }

    internal class MementoStorageObjectInternalFactory : IStorageObjectInternalFactory
    {
        private readonly StorageObjectMemento memento;

        public MementoStorageObjectInternalFactory(StorageObjectMemento memento)
        {
            this.memento = memento;
        }

        public StorageObject Create(StorageObjectInternalType storageObjectType)
        {
            return storageObjectType switch
            {
                StorageObjectInternalType.DataObject => new DataObject(this.memento),
                StorageObjectInternalType.RsaPrivateKeyObject => new RsaPrivateKeyObject(this.memento),
                StorageObjectInternalType.RsaPublicKeyObject => new RsaPublicKeyObject(this.memento),
                StorageObjectInternalType.X509CertificateObject => new X509CertificateObject(this.memento),
                StorageObjectInternalType.WtlsCertificateObject => new WtlsCertificateObject(this.memento),
                StorageObjectInternalType.X509AttributeCertificateObject => new X509AttributeCertificateObject(this.memento),
                StorageObjectInternalType.EcdsaPublicKeyObject => new EcdsaPublicKeyObject(this.memento),
                StorageObjectInternalType.EcdsaPrivateKeyObject => new EcdsaPrivateKeyObject(this.memento),
                StorageObjectInternalType.GenericSecretKeyObject => new GenericSecretKeyObject(this.memento),
                StorageObjectInternalType.AesKeyObject => new AesKeyObject(this.memento),
                _ => throw new InvalidProgramException($"Enum value {storageObjectType} is not supported.")
            };
        }
    }

    #endregion

    public static StorageObject CreateEmpty(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        System.Diagnostics.Debug.Assert(template != null);

        return CreateCore(template, new NewStorageObjectInternalFactory());
    }

    public static StorageObject CreateFromMemento(StorageObjectMemento memento)
    {
        System.Diagnostics.Debug.Assert(memento != null);
        return CreateCore(memento.Values, new MementoStorageObjectInternalFactory(memento));
    }

    public static StorageObject CreateFromMemento(StorageObjectMemento memento, bool validate)
    {
        System.Diagnostics.Debug.Assert(memento != null);
        StorageObject storageObject = CreateCore(memento.Values, new MementoStorageObjectInternalFactory(memento));

        if (validate)
        {
            storageObject.Validate();
        }

        return storageObject;
    }

    public static SecretKeyObject CreateSecret(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        System.Diagnostics.Debug.Assert(template != null);

        StorageObject storageObject = CreateSecretCore(template, new NewStorageObjectInternalFactory());
        return (SecretKeyObject)storageObject;
    }

    private static StorageObject CreateCore(IReadOnlyDictionary<CKA, IAttributeValue> template, IStorageObjectInternalFactory factory)
    {
        IAttributeValue? attributeValue;
        if (!template.TryGetValue(CKA.CKA_CLASS, out attributeValue))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE,
                $"Missing CKA_CLASS attribute in template.");
        }

        CKO classType = (CKO)attributeValue.AsUint();

        if (classType == CKO.CKO_DATA)
        {
            return factory.Create(StorageObjectInternalType.DataObject);
        }

        if (classType == CKO.CKO_PRIVATE_KEY)
        {
            CKK keyType = GetKeyType(template);

            if (keyType == CKK.CKK_RSA)
            {
                return factory.Create(StorageObjectInternalType.RsaPrivateKeyObject);
            }

            if (keyType == CKK.CKK_EC)
            {
                return factory.Create(StorageObjectInternalType.EcdsaPrivateKeyObject);
            }
        }

        if (classType == CKO.CKO_PUBLIC_KEY)
        {
            CKK keyType = GetKeyType(template);

            if (keyType == CKK.CKK_RSA)
            {
                return factory.Create(StorageObjectInternalType.RsaPublicKeyObject);
            }

            if (keyType == CKK.CKK_EC)
            {
                return factory.Create(StorageObjectInternalType.EcdsaPublicKeyObject);
            }
        }

        if (classType == CKO.CKO_CERTIFICATE)
        {
            CKC certificateType = GetCertificateType(template);
            return certificateType switch
            {
                CKC.CKC_X_509 => factory.Create(StorageObjectInternalType.X509CertificateObject),
                CKC.CKC_WTLS => factory.Create(StorageObjectInternalType.WtlsCertificateObject),
                CKC.CKC_X_509_ATTR_CERT => factory.Create(StorageObjectInternalType.X509AttributeCertificateObject),
                CKC.CKC_VENDOR_DEFINED => throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, "Value CKC_VENDOR_DEFINED for CKO_CERTIFICATE is not supported."),
                _ => throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"Value {certificateType} for CKO_CERTIFICATE is not defined.")
            };
        }

        if (classType == CKO.CKO_SECRET_KEY)
        {
            return CreateSecretCore(template, factory);
        }

        //TODO: Implement other object types.

        throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
            $"No match create object pattern (eg. CKA_CLASS, CKA_KEY_TYPE, CKA_CERTIFICATE_TYPE).");
    }

    private static StorageObject CreateSecretCore(IReadOnlyDictionary<CKA, IAttributeValue> template, IStorageObjectInternalFactory factory)
    {
        CKK keyType;
        if (template.ContainsKey(CKA.CKA_KEY_TYPE))
        {
            keyType = GetKeyType(template);
        }
        else
        {
            keyType = CKK.CKK_GENERIC_SECRET;
        }

        StorageObject storageObject = keyType switch
        {
            CKK.CKK_GENERIC_SECRET => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_MD5_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_SHA_1_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_SHA224_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_SHA256_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_SHA384_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_SHA512_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_RIPEMD128_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),
            CKK.CKK_RIPEMD160_HMAC => factory.Create(StorageObjectInternalType.GenericSecretKeyObject),

            CKK.CKK_AES => factory.Create(StorageObjectInternalType.AesKeyObject),
            _ => throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"Value {keyType} for CKO_SECRET_KEY is not defined.")
        };

        if (storageObject is GenericSecretKeyObject genericSeecret)
        {
            genericSeecret.CkaKeyType = keyType;
        }

        return storageObject;
    }

    private static CKK GetKeyType(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (!template.TryGetValue(CKA.CKA_KEY_TYPE, out IAttributeValue? attributeValue))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE,
                $"Missing CKA_KEY_TYPE attribute in template.");
        }

        return (CKK)attributeValue.AsUint();
    }

    private static CKC GetCertificateType(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (!template.TryGetValue(CKA.CKA_CERTIFICATE_TYPE, out IAttributeValue? attributeValue))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE,
                $"Missing CKA_CERTIFICATE_TYPE attribute in template.");
        }

        return (CKC)attributeValue.AsUint();
    }
}
