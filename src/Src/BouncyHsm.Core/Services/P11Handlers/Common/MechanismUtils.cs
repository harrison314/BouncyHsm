using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        | MechanismCkf.CKF_EC_ECPARAMETERS
        | MechanismCkf.CKF_EC_UNCOMPRESS;


    private readonly static Dictionary<CKM, MechanismInfo> originalMechanism;
    private static IDictionary<CKM, MechanismInfo> mechanism;
    private static string? profileName = null;

    private const int Poly1305KeySize = 32;
    private const int ChaCha20KeySize = 32;
    private const int Salsa20KeySize = 32;

    // Another mechanisms https://nshielddocs.entrust.com/api-generic/12.80/pkcs11
    static MechanismUtils()
    {
        originalMechanism = new Dictionary<CKM, MechanismInfo>()
        {
            // Digest algorithms
             { CKM.CKM_MD2, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_MD5, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA_1, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA224, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA256, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA384, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA512, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA512_T, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.CKF_DIGEST, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA512_256, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA512_224, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_RIPEMD128, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_RIPEMD160, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_GOSTR3411, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             { CKM.CKM_SHA3_256, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_SHA3_224, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_SHA3_384, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_SHA3_512, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_BLAKE2B_160, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_BLAKE2B_256, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_BLAKE2B_384, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
             { CKM.CKM_BLAKE2B_512, new MechanismInfo(0,0, MechanismCkf.CKF_DIGEST, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },

            // Generate Key pairs
            {CKM.CKM_RSA_PKCS_KEY_PAIR_GEN, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_GENERATE_KEY_PAIR, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RSA_X9_31_KEY_PAIR_GEN, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_GENERATE_KEY_PAIR, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
             
            // RSA PKCS1 
            {CKM.CKM_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_SIGN |  MechanismCkf.CKF_VERIFY | MechanismCkf.CKF_SIGN_RECOVER | MechanismCkf.CKF_VERIFY_RECOVER | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA1_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA224_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA256_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA384_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_MD2_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_MD5_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RIPEMD128_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RIPEMD160_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA3_224_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_256_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_384_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_512_RSA_PKCS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },

            {CKM.CKM_RSA_PKCS_OAEP, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40) },

            //CKM.CKM_SHA1_RSA_X9_31
            //{CKM.CKM_RSA_X9_31, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize,  MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA1_RSA_X9_31, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize,  MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },

            // RSA PSS
            {CKM.CKM_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY , Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA1_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA224_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA256_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA384_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, /*MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT |*/ MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA3_224_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_256_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_384_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_512_RSA_PKCS_PSS, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | /*MechanismCkf.CKF_SIGN_RECOVER |*/ MechanismCkf.CKF_VERIFY /*| MechanismCkf.CKF_VERIFY_RECOVER*/ /*| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP*/, MechanismCkf.CKF_SIGN|MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },

            //RSA ISO 9796-2
            {CKM.CKM_RSA_9796, new MechanismInfo(RsaMinKeySize, RsaMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY | MechanismCkf.CKF_SIGN_RECOVER | MechanismCkf.CKF_VERIFY_RECOVER , MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },

            // Generate EC Key pair
            {CKM.CKM_ECDSA_KEY_PAIR_GEN, new MechanismInfo(EcMinKeySize, EcMaxKeySize, MechanismCkf.CKF_GENERATE_KEY_PAIR | MechanismCkf.CKF_EC_NAMEDCURVE | MechanismCkf.CKF_EC_UNCOMPRESS | MechanismCkf.CKF_EC_ECPARAMETERS, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },

            {CKM.CKM_ECDSA, new MechanismInfo(EcMinKeySize, EcMaxKeySize,EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDSA_SHA1, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDSA_SHA224, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDSA_SHA256, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDSA_SHA384, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDSA_SHA512, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDSA_SHA3_224, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_ECDSA_SHA3_256, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_ECDSA_SHA3_384, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_ECDSA_SHA3_512, new MechanismInfo(EcMinKeySize, EcMaxKeySize, EcdsaSignVerify, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },

            // Generate symetric keys
            {CKM.CKM_GENERIC_SECRET_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA_1_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA224_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA256_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA384_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA512_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA512_224_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA512_256_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA512_T_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_224_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_256_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_384_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_512_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_160_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_256_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_384_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_512_KEY_GEN, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },

            // Hmacing
            {CKM.CKM_MD2_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_MD5_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RIPEMD128_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RIPEMD160_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA_1_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA224_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA256_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA384_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_224_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_256_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA3_256_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_224_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_384_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_512_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_160_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_256_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_384_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_512_HMAC, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
           

            //General hmacing
            {CKM.CKM_MD2_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_MD5_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RIPEMD128_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_RIPEMD160_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA_1_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA224_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA256_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA384_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_224_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_256_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA3_256_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_224_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_384_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_512_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_160_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_256_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_384_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_512_HMAC_GENERAL, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, Pkcs11SpecVersion.V3_0) },

            // Derive using digest
            {CKM.CKM_MD2_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_MD5_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA1_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA224_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA256_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA384_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_224_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA512_256_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_SHA3_256_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_224_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_384_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SHA3_512_KEY_DERIVATION, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_160_KEY_DERIVE, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_256_KEY_DERIVE, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_384_KEY_DERIVE, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_BLAKE2B_512_KEY_DERIVE, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },


            // Derive using data
            {CKM.CKM_CONCATENATE_BASE_AND_DATA, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_CONCATENATE_DATA_AND_BASE, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_XOR_BASE_AND_DATA, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_CONCATENATE_BASE_AND_KEY, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_EXTRACT_KEY_FROM_KEY, new MechanismInfo(1, SecretMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },

            {CKM.CKM_ECDH1_DERIVE, new MechanismInfo(EcMinKeySize, EcMaxKeySize, MechanismCkf.CKF_DERIVE | MechanismCkf.CKF_EC_NAMEDCURVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_ECDH1_COFACTOR_DERIVE, new MechanismInfo(EcMinKeySize, EcMaxKeySize, MechanismCkf.CKF_DERIVE | MechanismCkf.CKF_EC_NAMEDCURVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },

            // Derive using AES
            {CKM.CKM_AES_ECB_ENCRYPT_DATA,  new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_AES_CBC_ENCRYPT_DATA,  new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_DERIVE, MechanismCkf.CKF_DERIVE, Pkcs11SpecVersion.V2_40) },
            

            // AES
            {CKM.CKM_AES_KEY_GEN, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_ECB, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CBC, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CBC_PAD, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CFB1, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CFB8, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CFB64, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CFB128, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_OFB, new MechanismInfo(AesMinKeySize, AesMaxKeySize,  MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CTR, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, Pkcs11SpecVersion.V2_40)},
            {CKM.CKM_AES_CTS, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT, Pkcs11SpecVersion.V2_40)},


            {CKM.CKM_AES_GCM, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40) },
            {CKM.CKM_AES_CCM, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT | MechanismCkf.CKF_DECRYPT| MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V2_40) },

            {CKM.CKM_AES_KEY_WRAP_PAD, new MechanismInfo(AesMinKeySize, AesMaxKeySize, MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.NONE, Pkcs11SpecVersion.V2_40) },
            
            //POLY1305
            {CKM.CKM_POLY1305_KEY_GEN, new MechanismInfo(Poly1305KeySize, Poly1305KeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_POLY1305, new MechanismInfo(Poly1305KeySize, Poly1305KeySize, MechanismCkf.CKF_SIGN | MechanismCkf.CKF_VERIFY, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },


            //ChaCha20
            {CKM.CKM_CHACHA20_KEY_GEN, new MechanismInfo(ChaCha20KeySize, ChaCha20KeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_CHACHA20, new MechanismInfo(ChaCha20KeySize, ChaCha20KeySize, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_CHACHA20_POLY1305, new MechanismInfo(ChaCha20KeySize, ChaCha20KeySize, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT, Pkcs11SpecVersion.V3_0) },

            // Salsa20
            {CKM.CKM_SALSA20_KEY_GEN, new MechanismInfo(Salsa20KeySize, Salsa20KeySize, MechanismCkf.CKF_GENERATE, MechanismCkf.NONE, Pkcs11SpecVersion.V3_0) },
            {CKM.CKM_SALSA20, new MechanismInfo(Salsa20KeySize, Salsa20KeySize, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT | MechanismCkf.CKF_WRAP | MechanismCkf.CKF_UNWRAP, Pkcs11SpecVersion.V3_0) },
            //{CKM.CKM_SALSA20_POLY1305, new MechanismInfo(Salsa20KeySize, Salsa20KeySize, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT, MechanismCkf.CKF_ENCRYPT| MechanismCkf.CKF_DECRYPT, Pkcs11SpecVersion.V3_0) },

        };

        mechanism = originalMechanism;
    }

    public static uint[] GetMechanismAsUintArray()
    {
        IDictionary<CKM, MechanismInfo> localMechanism = mechanism;
        uint[] array = new uint[localMechanism.Count];

        int i = 0;
        foreach (CKM mechanism in localMechanism.Keys)
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

    public static bool IsUnwrapMechanismWithExplicitPading(CKM mechanismType)
    {
#if DEBUG
        if (!mechanism.TryGetValue(mechanismType, out MechanismInfo info)
            || !info.Flags.HasFlag(MechanismCkf.CKF_UNWRAP))
        {
            throw new ArgumentException($"Mechanism {mechanismType} is not for unwrap", nameof(mechanismType));
        }
#endif

        return mechanismType switch
        {
            CKM.CKM_AES_CBC => true,
            CKM.CKM_AES_ECB => true,
            CKM.CKM_AES_OFB => true,
            CKM.CKM_AES_CFB64 => true,
            CKM.CKM_AES_CFB8 => true,
            CKM.CKM_AES_CFB128 => true,
            CKM.CKM_AES_CFB1 => true,
            _ => false
        };
    }

    public static bool IsVendorDefined(CKM mechanismType)
    {
        return (CKM.CKM_VENDOR_DEFINED & mechanismType) == CKM.CKM_VENDOR_DEFINED;
    }

    public static string? GetProfileName()
    {
        return profileName;
    }

    public static void UpdateMechanisms<T>(Func<Dictionary<CKM, MechanismInfo>, T, MechanismProfile> updateFunction, T context)
    {
        MechanismProfile profile = updateFunction(originalMechanism, context);
        mechanism = profile.Mechanisms;
        profileName = profile.Name;
    }

    public static void ResetMechanism()
    {
        mechanism = originalMechanism;
        profileName = null;
    }
}
