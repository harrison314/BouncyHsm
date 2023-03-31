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

    public static string? ToPrintable(this IAttributeValue attributeValue, CKA attributeType)
    {
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

            _ => value.ToString()
        };
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