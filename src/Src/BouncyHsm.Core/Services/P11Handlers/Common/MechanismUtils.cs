using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class MechanismUtils
{
    private const uint RsaMinKeySize = 2048;
    private const uint RsaMaxKeySize = 6144;

    private const uint EcMinKeySize = 192;
    private const uint EcMaxKeySize = 521;

    private const uint SecretMaxKeySize = 10 * 1024 * 1024;

    private const uint AesMinKeySize = 16;
    private const uint AesMaxKeySize = 32;

    private const MechanismCkf EcdsaSignVerify = MechanismCkf.CKF_SIGN
        | MechanismCkf.CKF_VERIFY
        | MechanismCkf.CKF_EC_NAMEDCURVE
        | MechanismCkf.CKF_EC_UNCOMPRESS
        | MechanismCkf.CKF_EC_COMPRESS;


    private static Dictionary<CKM, MechanismInfo> mechanism;

    // Another mechanisms https://nshielddocs.entrust.com/api-generic/12.80/pkcs11
    static MechanismUtils()
    {
        mechanism = new Dictionary<CKM, MechanismInfo>()
        {
            // Digest algorithms
             { CKM.CKM_MD2, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_MD5, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA_1, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA224, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA256, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA384, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA512, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA512_T, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.CKF_DIGEST) },
             { CKM.CKM_SHA512_256, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_SHA512_224, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_RIPEMD128, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_RIPEMD160, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },
             { CKM.CKM_GOSTR3411, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE) },

            // Generate Key pairs
            {CKM.CKM_RSA_PKCS_KEY_PAIR_GEN, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_GENERATE_KEY_PAIR, MechanismCkf.NONE)},
            {CKM.CKM_RSA_X9_31_KEY_PAIR_GEN, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_GENERATE_KEY_PAIR, MechanismCkf.NONE)},
             
            // RSA PKCS1 
            {CKM.CKM_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.NONE) },
            {CKM.CKM_SHA1_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_SHA224_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_SHA256_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_SHA384_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_SHA512_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_MD2_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_MD5_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_RIPEMD128_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },
            {CKM.CKM_RIPEMD160_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE) },

            {CKM.CKM_RSA_PKCS_OAEP, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP) },


            // RSA PSS
            {CKM.CKM_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY )},
            {CKM.CKM_SHA1_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA224_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA256_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA384_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA512_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY)},

            // Generate EC Key pair
            {CKM.CKM_ECDSA_KEY_PAIR_GEN, new MechanismInfo(EcMinKeySize, EcMaxKeySize, MechanismCkf.CKF_GENERATE_KEY_PAIR|MechanismCkf.CKF_EC_NAMEDCURVE| MechanismCkf.CKF_EC_UNCOMPRESS, MechanismCkf.NONE)},

            {CKM.CKM_ECDSA, new MechanismInfo(EcMinKeySize, EcMaxKeySize,EcdsaSignVerify, MechanismCkf.NONE)},
            {CKM.CKM_ECDSA_SHA1, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE)},
            {CKM.CKM_ECDSA_SHA224, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE)},
            {CKM.CKM_ECDSA_SHA256, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE)},
            {CKM.CKM_ECDSA_SHA384, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE)},
            {CKM.CKM_ECDSA_SHA512, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE)},

            // Generate symetric keys
            {CKM.CKM_GENERIC_SECRET_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE)},

            // Hmacing
            {CKM.CKM_MD2_HMAC, new MechanismInfo(16, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_MD5_HMAC, new MechanismInfo(20, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_RIPEMD128_HMAC, new MechanismInfo(16, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_RIPEMD160_HMAC, new MechanismInfo(20, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA_1_HMAC, new MechanismInfo(20, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA224_HMAC, new MechanismInfo(28, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA256_HMAC, new MechanismInfo(32, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA384_HMAC, new MechanismInfo(48, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA512_HMAC, new MechanismInfo(64, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA512_224_HMAC, new MechanismInfo(28, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},
            {CKM.CKM_SHA512_256_HMAC, new MechanismInfo(32, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE)},

            //General hmacing
            {CKM.CKM_MD2_HMAC_GENERAL, new MechanismInfo(16, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_MD5_HMAC_GENERAL, new MechanismInfo(20, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_RIPEMD128_HMAC_GENERAL, new MechanismInfo(16, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_RIPEMD160_HMAC_GENERAL, new MechanismInfo(20, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA_1_HMAC_GENERAL, new MechanismInfo(20, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA224_HMAC_GENERAL, new MechanismInfo(28, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA256_HMAC_GENERAL, new MechanismInfo(32, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA384_HMAC_GENERAL, new MechanismInfo(48, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA512_HMAC_GENERAL, new MechanismInfo(64, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA512_224_HMAC_GENERAL, new MechanismInfo(28, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},
            {CKM.CKM_SHA512_256_HMAC_GENERAL, new MechanismInfo(32, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY)},

            // Derive using digest
            {CKM.CKM_MD2_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_MD5_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA1_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA224_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA256_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA384_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA512_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA512_224_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},
            {CKM.CKM_SHA512_256_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE)},

            // Derive using data
            {CKM.CKM_CONCATENATE_BASE_AND_DATA, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},
            {CKM.CKM_CONCATENATE_DATA_AND_BASE, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},
            {CKM.CKM_XOR_BASE_AND_DATA, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},
            {CKM.CKM_CONCATENATE_BASE_AND_KEY, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},
            {CKM.CKM_EXTRACT_KEY_FROM_KEY, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},

            {CKM.CKM_ECDH1_DERIVE, new MechanismInfo(EcMinKeySize, EcMaxKeySize, MechanismCkf.CKF_DERIVE | MechanismCkf.CKF_EC_NAMEDCURVE, MechanismCkf.CKF_DERIVE)},
            {CKM.CKM_ECDH1_COFACTOR_DERIVE, new MechanismInfo(EcMinKeySize, EcMaxKeySize, MechanismCkf.CKF_DERIVE | MechanismCkf.CKF_EC_NAMEDCURVE, MechanismCkf.CKF_DERIVE)},

            // Derive using AES
            {CKM.CKM_AES_ECB_ENCRYPT_DATA,  new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},
            {CKM.CKM_AES_CBC_ENCRYPT_DATA,  new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE)},
            
            // AES
            {CKM.CKM_AES_KEY_GEN, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE)},
            {CKM.CKM_AES_ECB, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.NONE)},
            {CKM.CKM_AES_CBC, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_CBC_PAD, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP)},
            {CKM.CKM_AES_CFB1, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT , MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_CFB8, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_CFB64, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_CFB128, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_OFB, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_CTR, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},
            {CKM.CKM_AES_CTS, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT)},

            {CKM.CKM_AES_GCM, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP)},
            {CKM.CKM_AES_CCM, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP)},

            {CKM.CKM_AES_KEY_WRAP_PAD, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.NONE)},

        };
    }

    public static uint[] GetMechanismAsUintArray()
    {
        uint[] array = new uint[mechanism.Count];

        int i = 0;
        foreach (CKM mechanism in mechanism.Keys)
        {
            array[i] = (uint)mechanism;
            i++;
        }

        Array.Sort(array);

        return array;
    }

    public static bool TryGetMechanismInfo(CKM mechanismValue, out MechanismInfo info)
    {
        return mechanism.TryGetValue(mechanismValue, out info);
    }

    public static bool TryGetMechanismInfo(uint mechanismValue, out MechanismInfo info)
    {
        return mechanism.TryGetValue((CKM)mechanismValue, out info);
    }

    public static bool IsUsageTo(CKM mechanismValue, MechanismCkf usage)
    {
        if (!mechanism.TryGetValue(mechanismValue, out MechanismInfo info))
        {
            return false;
        }

        return info.Flags.HasFlag(usage);
    }

    public static bool RequireParamsTo(CKM mechanismValue, MechanismCkf usage)
    {
        if (!mechanism.TryGetValue(mechanismValue, out MechanismInfo info))
        {
            return false;
        }

        return info.RequireParamsIn.HasFlag(usage);
    }

    public static void CheckMechanism(BouncyHsm.Core.Rpc.MechanismValue mechanismValue, MechanismCkf usage)
    {
        CKM ckMechanism = (CKM)mechanismValue.MechanismType;

        if (!mechanism.TryGetValue(ckMechanism, out MechanismInfo info))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"{ckMechanism} is not supported.");
        }

        if (!info.Flags.HasFlag(usage))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"{ckMechanism} is not usage for {usage}.");
        }

        if (info.RequireParamsIn.HasFlag(usage))
        {
            if (mechanismValue.MechanismParamMp == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"{ckMechanism} requires params.");
            }
        }
        else
        {
            if (mechanismValue.MechanismParamMp != null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"In digest algorithm {ckMechanism} required empty mechanism params.");
            }
        }
    }

    public static bool IsVendorDefined(CKM mechanismType)
    {
        return (CKM.CKM_VENDOR_DEFINED & mechanismType) == CKM.CKM_VENDOR_DEFINED;
    }
}
