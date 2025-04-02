using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.Profiles;

public sealed class FilterFipsProfileOperation : ProfileOperation
{
    public FilterFipsProfileOperation()
    {

    }

    public override void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms)
    {
        List<CKM> mechanismsToDelete = new List<CKM>();
        HashSet<CKM> fipsMechanism = this.GetAllFipsMechanism();

        foreach ((CKM mechanism, _) in mechanisms)
        {
            if (!fipsMechanism.Contains(mechanism))
            {
                mechanismsToDelete.Add(mechanism);
            }
        }

        foreach (CKM mechanism in mechanismsToDelete)
        {
            mechanisms.Remove(mechanism);
        }
    }

    private HashSet<CKM> GetAllFipsMechanism()
    {
        HashSet<CKM> fips = new HashSet<CKM>()
        {
            CKM.CKM_AES_CBC,
            CKM.CKM_AES_CBC_PAD,
            CKM.CKM_AES_CCM,
            CKM.CKM_AES_CMAC,
            CKM.CKM_AES_CMAC_GENERAL,
            CKM.CKM_AES_ECB,
            CKM.CKM_AES_GCM,
            CKM.CKM_AES_KEY_GEN,
            CKM.CKM_AES_KEY_WRAP,
            CKM.CKM_AES_KEY_WRAP_PAD,
            CKM.CKM_AES_KEY_WRAP,
            CKM.CKM_AES_KEY_WRAP_PAD,
            CKM.CKM_AES_OFB,
            //CKM.CKM_DECODE_PKCS_7,
            //CKM.CKM_DECODE_X_509,
            CKM.CKM_DES3_CBC,
            CKM.CKM_DES3_CBC_PAD,
            CKM.CKM_DES3_CMAC,
            CKM.CKM_DES3_CMAC_GENERAL,
            CKM.CKM_DES3_ECB,
            //CKM.CKM_DES3_ECB_PAD,
            CKM.CKM_DES3_KEY_GEN,
            CKM.CKM_DES3_MAC,
            CKM.CKM_DES3_MAC_GENERAL,
            //CKM.CKM_DES3_OFB64,
            //CKM.CKM_DES3_RETAIL_CFB_MAC,
            //CKM.CKM_DES3_X919_MAC,
            //CKM.CKM_DES3_X919_MAC_GENERAL,
            CKM.CKM_DH_PKCS_DERIVE,
            CKM.CKM_DH_PKCS_KEY_PAIR_GEN,
            CKM.CKM_DH_PKCS_PARAMETER_GEN,
            CKM.CKM_DSA,
            CKM.CKM_DSA_KEY_PAIR_GEN,
            CKM.CKM_DSA_PARAMETER_GEN,
            CKM.CKM_DSA_SHA1,
            //CKM.CKM_DSA_SHA1_PKCS,
            CKM.CKM_DSA_SHA224,
            //CKM.CKM_DSA_SHA224_PKCS,
            CKM.CKM_DSA_SHA256,
            //CKM.CKM_DSA_SHA256_PKCS,
            CKM.CKM_DSA_SHA384,
            //CKM.CKM_DSA_SHA384_PKCS,
            CKM.CKM_DSA_SHA512,
            //CKM.CKM_DSA_SHA512_PKCS,
            CKM.CKM_EC_KEY_PAIR_GEN,
            CKM.CKM_ECDH1_DERIVE,
            CKM.CKM_ECDSA,
            //CKM.CKM_ECDSA_GBCS_SHA256,
            CKM.CKM_ECDSA_SHA1,
            //CKM.CKM_ECDSA_SHA3_224,
            //CKM.CKM_ECDSA_SHA3_256,
            //CKM.CKM_ECDSA_SHA3_384,
            //CKM.CKM_ECDSA_SHA3_512,
            CKM.CKM_ECDSA_SHA224,
            CKM.CKM_ECDSA_SHA256,
            CKM.CKM_ECDSA_SHA384,
            CKM.CKM_ECDSA_SHA512,
            //CKM.CKM_ENCODE_ATTRIBUTES,
            //CKM.CKM_ENCODE_PKCS_10,
            //CKM.CKM_ENCODE_PUBLIC_KEY,
            //CKM.CKM_ENCODE_X_509,
            //CKM.CKM_ENCODE_X_509_LOCAL_CERT,
            CKM.CKM_GENERIC_SECRET_KEY_GEN,
            //CKM.CKM_KECCAK_1600,
            //CKM.CKM_KEY_WRAP_SET_OAEP,
            //CKM.CKM_PP_LOAD_SECRET,
            //CKM.CKM_PP_LOAD_SECRET_2,
            //CKM.CKM_REPLICATE_TOKEN_RSA_AES,
            //CKM.CKM_RSA_FIPS_186_4_PRIME_KEY_PAIR_GEN,
            CKM.CKM_RSA_PKCS,
            CKM.CKM_RSA_PKCS_KEY_PAIR_GEN,
            CKM.CKM_RSA_PKCS_OAEP,
            CKM.CKM_RSA_PKCS_PSS,
            CKM.CKM_RSA_X9_31_KEY_PAIR_GEN,
            //CKM.CKM_SECRET_RECOVER_WITH_ATTRIBUTES,
            //CKM.CKM_SECRET_SHARE_WITH_ATTRIBUTES,
            //CKM.CKM_SET_ATTRIBUTES,
            CKM.CKM_SHA_1,
            CKM.CKM_SHA_1_HMAC,
            CKM.CKM_SHA_1_HMAC_GENERAL,
            CKM.CKM_SHA1_RSA_PKCS,
            CKM.CKM_SHA1_RSA_PKCS_PSS,
            //CKM.CKM_SHA3_224,
            //CKM.CKM_SHA3_224_HMAC,
            //CKM.CKM_SHA3_224_HMAC_GENERAL,
            //CKM.CKM_SHA3_224_RSA_PKCS,
            //CKM.CKM_SHA3_224_RSA_PKCS_PSS,
            //CKM.CKM_SHA3_256,
            //CKM.CKM_SHA3_256_HMAC,
            //CKM.CKM_SHA3_256_HMAC_GENERAL,
            //CKM.CKM_SHA3_256_RSA_PKCS,
            //CKM.CKM_SHA3_256_RSA_PKCS_PSS,
            //CKM.CKM_SHA3_384,
            //CKM.CKM_SHA3_384_HMAC,
            //CKM.CKM_SHA3_384_HMAC_GENERAL,
            //CKM.CKM_SHA3_384_RSA_PKCS,
            //CKM.CKM_SHA3_384_RSA_PKCS_PSS,
            //CKM.CKM_SHA3_512,
            //CKM.CKM_SHA3_512_HMAC,
            //CKM.CKM_SHA3_512_HMAC_GENERAL,
            //CKM.CKM_SHA3_512_RSA_PKCS,
            //CKM.CKM_SHA3_512_RSA_PKCS_PSS,
            CKM.CKM_SHA224,
            CKM.CKM_SHA224_HMAC,
            CKM.CKM_SHA224_HMAC_GENERAL,
            CKM.CKM_SHA224_RSA_PKCS,
            CKM.CKM_SHA224_RSA_PKCS_PSS,
            CKM.CKM_SHA256,
            CKM.CKM_SHA256_HMAC,
            CKM.CKM_SHA256_HMAC_GENERAL,
            CKM.CKM_SHA256_RSA_PKCS,
            CKM.CKM_SHA256_RSA_PKCS_PSS,
            CKM.CKM_SHA384,
            CKM.CKM_SHA384_HMAC,
            CKM.CKM_SHA384_HMAC_GENERAL,
            CKM.CKM_SHA384_RSA_PKCS,
            CKM.CKM_SHA384_RSA_PKCS_PSS,
            CKM.CKM_SHA512,
            CKM.CKM_SHA512_HMAC,
            CKM.CKM_SHA512_HMAC_GENERAL,
            CKM.CKM_SHA512_RSA_PKCS,
            CKM.CKM_SHA512_RSA_PKCS_PSS,
            CKM.CKM_SSL3_PRE_MASTER_KEY_GEN,
            //CKM.CKM_TDEA_TKW,
            //CKM.CKM_TUAK_DERIVE,
            //CKM.CKM_TUAK_SIGN,
            //CKM.CKM_WRAPKEY_AES_CBC,
            //CKM.CKM_WRAPKEY_AES_KWP,
            //CKM.CKM_WRAPKEY_DES3_CBC,
            //CKM.CKM_WRAPKEY_DES3_ECB,
            //CKM.CKM_WRAPKEYBLOB_AES_CBC,
            //CKM.CKM_WRAPKEYBLOB_DES3_CBC,
            CKM.CKM_X9_42_DH_DERIVE,
            CKM.CKM_X9_42_DH_KEY_PAIR_GEN,
            CKM.CKM_X9_42_DH_PARAMETER_GEN,
        };

        return fips;
    }
}