using BouncyHsm.Core.Services.Contracts;
using System.Text;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Utils;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Implementation;

internal static class AttributeValueExtensions
{
    public static string ToHexadecimal(this IAttributeValue attributeValue)
    {
        return attributeValue.TypeTag switch
        {
            AttrTypeTag.ByteArray => HexConvertor.GetString(attributeValue.AsByteArray()),
            AttrTypeTag.CkBool => attributeValue.AsBool() ? "01" : "00",
            AttrTypeTag.CkUint => attributeValue.AsUint().ToString("X8"),
            AttrTypeTag.String => HexConvertor.GetString(Encoding.UTF8.GetBytes(attributeValue.AsString())),
            AttrTypeTag.DateTime => HexConvertor.GetString(Encoding.UTF8.GetBytes(attributeValue.AsDate().ToString())),
            _ => throw new InvalidProgramException($"Enum value {attributeValue.TypeTag} is not supported.")
        };
    }

    public static string? ToPrintable(this IAttributeValue attributeValue, CKA attributeType, Dictionary<CKA, IAttributeValue> memenoto)
    {
        if (attributeType == CKA.CKA_PARAMETER_SET)
        {
            return ParameterSetToString(attributeValue, memenoto);
        }

        return attributeValue.TypeTag switch
        {
            AttrTypeTag.ByteArray when
                attributeType is CKA.CKA_AC_ISSUER or CKA.CKA_ISSUER or CKA.CKA_SUBJECT => TryDecodeSubject(attributeValue.AsByteArray()),
            AttrTypeTag.ByteArray => ByteArrayToPrintable(attributeValue.AsByteArray()),
            AttrTypeTag.CkBool => attributeValue.AsBool().ToString(),
            AttrTypeTag.CkUint => UintToString(attributeType, attributeValue.AsUint()),
            AttrTypeTag.String => attributeValue.AsString(),
            AttrTypeTag.DateTime => attributeValue.AsDate().ToString(),
            _ => throw new InvalidProgramException($"Enum value {attributeValue.TypeTag} is not supported.")
        };
    }

    private static string? TryDecodeSubject(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return null;
        }

        try
        {
            return Org.BouncyCastle.Asn1.X509.X509Name.GetInstance(bytes).ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string UintToString(CKA attributeType, uint value)
    {
        return attributeType switch
        {
            CKA.CKA_CLASS => ((CKO)value).ToString(),
            CKA.CKA_CERTIFICATE_TYPE => ((CKC)value).ToString(),
            CKA.CKA_KEY_GEN_MECHANISM => ((CKM)value).ToString(),
            CKA.CKA_MECHANISM_TYPE => ((CKM)value).ToString(),
            CKA.CKA_KEY_TYPE => ((CKK)value).ToString(),
            CKA.CKA_NAME_HASH_ALGORITHM => ((CKM)value).ToString(),
            CKA.CKA_CERTIFICATE_CATEGORY => ((CKCertificateCategory)value).ToString(),
            CKA.CKA_TRUST_SERVER_AUTH => ((CKT)value).ToString(),
            CKA.CKA_TRUST_CLIENT_AUTH => ((CKT)value).ToString(),
            CKA.CKA_TRUST_CODE_SIGNING => ((CKT)value).ToString(),
            CKA.CKA_TRUST_EMAIL_PROTECTION => ((CKT)value).ToString(),
            CKA.CKA_TRUST_IPSEC_IKE => ((CKT)value).ToString(),
            CKA.CKA_TRUST_TIME_STAMPING => ((CKT)value).ToString(),
            CKA.CKA_TRUST_OCSP_SIGNING => ((CKT)value).ToString(),

            _ => value.ToString()
        };
    }

    private static string ParameterSetToString(IAttributeValue attributeValue, Dictionary<CKA, IAttributeValue> memenoto)
    {
        if (memenoto.TryGetValue(CKA.CKA_KEY_TYPE, out IAttributeValue? keyPyteAttr))
        {
            CKK keyType = (CKK)keyPyteAttr.AsUint();
            if (keyType == CKK.CKK_ML_DSA)
            {
                return ((CK_ML_DSA_PARAMETER_SET)attributeValue.AsUint()).ToString();
            }

            if (keyType == CKK.CKK_SLH_DSA)
            {
                return ((CK_SLH_DSA_PARAMETER_SET)attributeValue.AsUint()).ToString();
            }

            if (keyType == CKK.CKK_ML_KEM)
            {
                return ((CK_ML_KEM_PARAMETER_SET)attributeValue.AsUint()).ToString();
            }
        }

        return attributeValue.AsUint().ToString();
    }

    private static string? ByteArrayToPrintable(Span<byte> array)
    {
        if (array.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < array.Length; i++)
        {
            char c = (char)array[i];
            if (!IsPrintableChar(c))
            {
                return null;
            }
        }

        return Encoding.UTF8.GetString(array);
    }

    private static bool IsPrintableChar(char ch)
    {
        return char.IsLetterOrDigit(ch)
            || char.IsPunctuation(ch)
            //|| char.IsSymbol(ch)
            || (ch == ' ')
            || (ch == '\t')
            || (ch == '\r')
            || (ch == '\n');
    }
}