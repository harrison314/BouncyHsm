using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class AttrTypeUtils
{
    public static AttrTypeTag GetTypeTag(CKA attributeType)
    {
        return attributeType switch
        {
            CKA.CKA_CLASS => AttrTypeTag.CkUint,
            CKA.CKA_TOKEN => AttrTypeTag.CkBool,
            CKA.CKA_PRIVATE => AttrTypeTag.CkBool,
            CKA.CKA_LABEL => AttrTypeTag.String,
            CKA.CKA_APPLICATION => AttrTypeTag.String,
            CKA.CKA_VALUE => AttrTypeTag.ByteArray,
            CKA.CKA_OBJECT_ID => AttrTypeTag.ByteArray,
            CKA.CKA_CERTIFICATE_TYPE => AttrTypeTag.CkUint,
            CKA.CKA_ISSUER => AttrTypeTag.ByteArray,
            CKA.CKA_SERIAL_NUMBER => AttrTypeTag.ByteArray,
            CKA.CKA_AC_ISSUER => AttrTypeTag.ByteArray,
            CKA.CKA_OWNER => AttrTypeTag.ByteArray,
            CKA.CKA_ATTR_TYPES => AttrTypeTag.ByteArray,
            CKA.CKA_TRUSTED => AttrTypeTag.CkBool,
            CKA.CKA_CERTIFICATE_CATEGORY => AttrTypeTag.CkUint,
            CKA.CKA_JAVA_MIDP_SECURITY_DOMAIN => AttrTypeTag.CkUint,
            CKA.CKA_URL => AttrTypeTag.String,
            CKA.CKA_HASH_OF_SUBJECT_PUBLIC_KEY => AttrTypeTag.ByteArray,
            CKA.CKA_HASH_OF_ISSUER_PUBLIC_KEY => AttrTypeTag.ByteArray,
            CKA.CKA_CHECK_VALUE => AttrTypeTag.ByteArray,
            CKA.CKA_KEY_TYPE => AttrTypeTag.CkUint,
            CKA.CKA_SUBJECT => AttrTypeTag.ByteArray,
            CKA.CKA_ID => AttrTypeTag.ByteArray,
            CKA.CKA_SENSITIVE => AttrTypeTag.CkBool,
            CKA.CKA_ENCRYPT => AttrTypeTag.CkBool,
            CKA.CKA_DECRYPT => AttrTypeTag.CkBool,
            CKA.CKA_WRAP => AttrTypeTag.CkBool,
            CKA.CKA_UNWRAP => AttrTypeTag.CkBool,
            CKA.CKA_SIGN => AttrTypeTag.CkBool,
            CKA.CKA_SIGN_RECOVER => AttrTypeTag.CkBool,
            CKA.CKA_VERIFY => AttrTypeTag.CkBool,
            CKA.CKA_VERIFY_RECOVER => AttrTypeTag.CkBool,
            CKA.CKA_DERIVE => AttrTypeTag.CkBool,
            CKA.CKA_START_DATE => AttrTypeTag.DateTime,
            CKA.CKA_END_DATE => AttrTypeTag.DateTime,
            CKA.CKA_MODULUS => AttrTypeTag.ByteArray,
            CKA.CKA_MODULUS_BITS => AttrTypeTag.CkUint,
            CKA.CKA_PUBLIC_EXPONENT => AttrTypeTag.ByteArray,
            CKA.CKA_PRIVATE_EXPONENT => AttrTypeTag.ByteArray,
            CKA.CKA_PRIME_1 => AttrTypeTag.ByteArray,
            CKA.CKA_PRIME_2 => AttrTypeTag.ByteArray,
            CKA.CKA_EXPONENT_1 => AttrTypeTag.ByteArray,
            CKA.CKA_EXPONENT_2 => AttrTypeTag.ByteArray,
            CKA.CKA_COEFFICIENT => AttrTypeTag.ByteArray,
            CKA.CKA_PUBLIC_KEY_INFO => AttrTypeTag.ByteArray,
            CKA.CKA_PRIME => AttrTypeTag.ByteArray,
            CKA.CKA_SUBPRIME => AttrTypeTag.ByteArray,
            CKA.CKA_BASE => AttrTypeTag.ByteArray,
            CKA.CKA_PRIME_BITS => AttrTypeTag.CkUint,
            CKA.CKA_SUBPRIME_BITS => AttrTypeTag.CkUint,
            CKA.CKA_VALUE_BITS => AttrTypeTag.CkUint,
            CKA.CKA_VALUE_LEN => AttrTypeTag.CkUint,
            CKA.CKA_EXTRACTABLE => AttrTypeTag.CkBool,
            CKA.CKA_LOCAL => AttrTypeTag.CkBool,
            CKA.CKA_NEVER_EXTRACTABLE => AttrTypeTag.CkBool,
            CKA.CKA_ALWAYS_SENSITIVE => AttrTypeTag.CkBool,
            CKA.CKA_KEY_GEN_MECHANISM => AttrTypeTag.CkUint,
            CKA.CKA_MODIFIABLE => AttrTypeTag.CkBool,
            CKA.CKA_COPYABLE => AttrTypeTag.CkBool,
            CKA.CKA_DESTROYABLE => AttrTypeTag.CkBool,
            CKA.CKA_ECDSA_PARAMS => AttrTypeTag.ByteArray,
            //CKA.CKA_EC_PARAMS => AttrTypeTag.ByteArray ,
            CKA.CKA_EC_POINT => AttrTypeTag.ByteArray,
            CKA.CKA_SECONDARY_AUTH => AttrTypeTag.CkBool,
            CKA.CKA_AUTH_PIN_FLAGS => AttrTypeTag.CkUint,
            CKA.CKA_ALWAYS_AUTHENTICATE => AttrTypeTag.CkBool,
            CKA.CKA_WRAP_WITH_TRUSTED => AttrTypeTag.CkBool,
            CKA.CKA_WRAP_TEMPLATE => AttrTypeTag.CkAttributeArray,
            CKA.CKA_UNWRAP_TEMPLATE => AttrTypeTag.CkAttributeArray,
            CKA.CKA_DERIVE_TEMPLATE => AttrTypeTag.CkAttributeArray,
            CKA.CKA_OTP_FORMAT => AttrTypeTag.CkUint,
            CKA.CKA_OTP_LENGTH => AttrTypeTag.CkUint,
            CKA.CKA_OTP_TIME_INTERVAL => AttrTypeTag.CkUint,
            CKA.CKA_OTP_USER_FRIENDLY_MODE => AttrTypeTag.CkBool,
            CKA.CKA_OTP_CHALLENGE_REQUIREMENT => AttrTypeTag.CkUint,
            CKA.CKA_OTP_TIME_REQUIREMENT => AttrTypeTag.CkUint,
            CKA.CKA_OTP_COUNTER_REQUIREMENT => AttrTypeTag.CkUint,
            CKA.CKA_OTP_PIN_REQUIREMENT => AttrTypeTag.CkUint,
            CKA.CKA_OTP_COUNTER => AttrTypeTag.ByteArray,
            CKA.CKA_OTP_TIME => AttrTypeTag.String,
            CKA.CKA_OTP_USER_IDENTIFIER => AttrTypeTag.String,
            CKA.CKA_OTP_SERVICE_IDENTIFIER => AttrTypeTag.String,
            CKA.CKA_OTP_SERVICE_LOGO => AttrTypeTag.ByteArray,
            CKA.CKA_OTP_SERVICE_LOGO_TYPE => AttrTypeTag.String,
            CKA.CKA_GOSTR3410_PARAMS => AttrTypeTag.ByteArray,
            CKA.CKA_GOSTR3411_PARAMS => AttrTypeTag.ByteArray,
            CKA.CKA_GOST28147_PARAMS => AttrTypeTag.ByteArray,
            CKA.CKA_HW_FEATURE_TYPE => AttrTypeTag.CkUint,
            CKA.CKA_RESET_ON_INIT => AttrTypeTag.CkBool,
            CKA.CKA_HAS_RESET => AttrTypeTag.CkBool,
            CKA.CKA_PIXEL_X => AttrTypeTag.CkUint,
            CKA.CKA_PIXEL_Y => AttrTypeTag.CkUint,
            CKA.CKA_RESOLUTION => AttrTypeTag.CkUint,
            CKA.CKA_CHAR_ROWS => AttrTypeTag.CkUint,
            CKA.CKA_CHAR_COLUMNS => AttrTypeTag.CkUint,
            CKA.CKA_COLOR => AttrTypeTag.CkBool,
            CKA.CKA_BITS_PER_PIXEL => AttrTypeTag.CkUint,
            CKA.CKA_CHAR_SETS => AttrTypeTag.String,
            CKA.CKA_ENCODING_METHODS => AttrTypeTag.String,
            CKA.CKA_MIME_TYPES => AttrTypeTag.String,
            CKA.CKA_MECHANISM_TYPE => AttrTypeTag.CkUint,
            CKA.CKA_REQUIRED_CMS_ATTRIBUTES => AttrTypeTag.ByteArray,
            CKA.CKA_DEFAULT_CMS_ATTRIBUTES => AttrTypeTag.ByteArray,
            CKA.CKA_SUPPORTED_CMS_ATTRIBUTES => AttrTypeTag.ByteArray,
            CKA.CKA_ALLOWED_MECHANISMS => AttrTypeTag.CkAttributeArray,
            CKA.CKA_NAME_HASH_ALGORITHM => AttrTypeTag.CkUint,
            CKA.CKA_VENDOR_DEFINED => throw new InvalidOperationException("value is not defined"),
            _ => throw new InvalidProgramException($"Enum value {attributeType} is not supported.")
        };
    }

    public static Dictionary<CKA, IAttributeValue> BuildDictionaryTemplate(AttrValueFromNative[] template)
    {
        CKA lastCka = CKA.CKA_CLASS;
        try
        {
            Dictionary<CKA, IAttributeValue> dictTemplate = new Dictionary<CKA, IAttributeValue>();
            foreach (AttrValueFromNative attr in template)
            {
                lastCka = (CKA)attr.AttributeType;
                dictTemplate.Add(lastCka, new NativeAttributeValue(attr));
            }

            return dictTemplate;
        }
        catch (ArgumentException ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                $"Duplicate attribute type {lastCka} in template.",
                ex);
        }
    }
}
