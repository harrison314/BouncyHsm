using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal static class CryptoObjectValueChecker
{
    public static void CheckNotEmpty(CKA attributeType, byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length == 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
               $"Attribute {attributeType} can not by empty.");
        }
    }

    public static void CheckDerObjectIdentifier(CKA attributeType, byte[] data, bool enableEmpty)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length == 0)
        {
            if (!enableEmpty)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} can not by empty.");
            }

            return;
        }

        try
        {
            _ = Org.BouncyCastle.Asn1.DerObjectIdentifier.GetInstance(data);
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"Attribute {attributeType} is not valid OID in DER encoding.",
                ex);
        }
    }

    public static void CheckPublicSubjectKeyInfo(CKA attributeType, byte[] data, bool enableEmpty)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length == 0)
        {
            if (!enableEmpty)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} can not by empty.");
            }

            return;
        }

        try
        {
            _ = Org.BouncyCastle.Asn1.X509.SubjectPublicKeyInfo.GetInstance(data);
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"Attribute {attributeType} is not valid Public subject key info in DER encoding.",
                ex);
        }
    }

    public static void CheckX509DerCertificate(CKA attributeType, byte[] data, bool enableEmpty)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length == 0)
        {
            if (!enableEmpty)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} can not by empty.");
            }

            return;
        }

        if (data[0] != 0x30) // Starts with ASN.1 sequence
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} is not valid X509 certificate with DER encodng.");
        }

        try
        {
            Org.BouncyCastle.X509.X509CertificateParser parser = new Org.BouncyCastle.X509.X509CertificateParser();
            _ = parser.ReadCertificate(data);
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"Attribute {attributeType} is not valid X509 certificate with DER encodng.",
                ex);
        }
    }

    public static void CheckX509Name(CKA attributeType, byte[] data, bool enableEmpty)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length == 0)
        {
            if (!enableEmpty)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} can not by empty.");
            }

            return;
        }

        try
        {
            _ = Org.BouncyCastle.Asn1.X509.X509Name.GetInstance(data);
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"Attribute {attributeType} is not valid X509 Name in DER encoding.",
                ex);
        }
    }

    public static void CheckStartEndDate(CkDate start, CkDate end)
    {
        if (start.HasValue && end.HasValue)
        {
            if (start.Value > end.Value)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                  $"Attribute {CKA.CKA_START_DATE} must by less than {CKA.CKA_END_DATE}.");
            }
        }
    }

    public static void CheckIsCheckValue(CKA attributeType, byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length != 0 && data.Length != 3)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                  $"Attribute {attributeType} must by empty or 3 byte length.");
        }
    }

    public static void CheckDigestValue(CKA attributeType, CKM digestMechnaism, byte[] digest, bool enableEmpty)
    {
        System.Diagnostics.Debug.Assert(digest != null);

        if (digest.Length == 0)
        {
            if (!enableEmpty)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} can not by empty.");
            }

            return;
        }

        Org.BouncyCastle.Crypto.IDigest? algorithm = BouncyHsm.Core.Services.P11Handlers.Common.DigestUtils.TryGetDigest(digestMechnaism);
        if (algorithm == null)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {CKA.CKA_NAME_HASH_ALGORITHM} is invalid mechanism.");
        }

        int digestSize = algorithm.GetDigestSize();
        if (digestSize != digest.Length)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} has invalid digest lenght ({digest.Length}), excepted is {digestSize} for {digestMechnaism}.");
        }
    }

    public static void CheckDerInteger(CKA attributeType, byte[] data, bool enableEmpty, bool mustByPositive)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (data.Length == 0)
        {
            if (!enableEmpty)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} can not by empty.");
            }

            return;
        }

        try
        {
            Org.BouncyCastle.Asn1.DerInteger value = Org.BouncyCastle.Asn1.DerInteger.GetInstance(data);
            if (mustByPositive && value.Value.SignValue < 0)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                   $"Attribute {attributeType} must by positive DER integer.");
            }
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID,
                $"Attribute {attributeType} is not valid X509 Name in DER encoding.",
                ex);
        }
    }
}
