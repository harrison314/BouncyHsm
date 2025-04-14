/* pkcs11t.h include file for PKCS #11. */
/* Extra headers */


#ifndef _PKCS11T_V3_1_H_
#define _PKCS11T_V3_1_H_ 1

#define CKG_MGF1_SHA3_224     0x00000006UL
#define CKG_MGF1_SHA3_256     0x00000007UL
#define CKG_MGF1_SHA3_384     0x00000008UL
#define CKG_MGF1_SHA3_512     0x00000009UL


#define CKM_SHA3_256                   0x000002b0UL
#define CKM_SHA3_256_HMAC              0x000002b1UL
#define CKM_SHA3_256_HMAC_GENERAL      0x000002b2UL
#define CKM_SHA3_256_KEY_GEN           0x000002b3UL
#define CKM_SHA3_224                   0x000002b5UL
#define CKM_SHA3_224_HMAC              0x000002b6UL
#define CKM_SHA3_224_HMAC_GENERAL      0x000002b7UL
#define CKM_SHA3_224_KEY_GEN           0x000002b8UL
#define CKM_SHA3_384                   0x000002c0UL
#define CKM_SHA3_384_HMAC              0x000002c1UL
#define CKM_SHA3_384_HMAC_GENERAL      0x000002c2UL
#define CKM_SHA3_384_KEY_GEN           0x000002c3UL
#define CKM_SHA3_512                   0x000002d0UL
#define CKM_SHA3_512_HMAC              0x000002d1UL
#define CKM_SHA3_512_HMAC_GENERAL      0x000002d2UL
#define CKM_SHA3_512_KEY_GEN           0x000002d3UL

#define CKM_SHA_1_KEY_GEN              0x00004003UL
#define CKM_SHA224_KEY_GEN             0x00004004UL
#define CKM_SHA256_KEY_GEN             0x00004005UL
#define CKM_SHA384_KEY_GEN             0x00004006UL
#define CKM_SHA512_KEY_GEN             0x00004007UL
#define CKM_SHA512_224_KEY_GEN         0x00004008UL
#define CKM_SHA512_256_KEY_GEN         0x00004009UL
#define CKM_SHA512_T_KEY_GEN           0x0000400aUL

#define CKM_SHA3_256_KEY_DERIVATION    0x00000397UL
#define CKM_SHA3_224_KEY_DERIVATION    0x00000398UL
#define CKM_SHA3_384_KEY_DERIVATION    0x00000399UL
#define CKM_SHA3_512_KEY_DERIVATION    0x0000039aUL

#define CKM_SHA3_256_RSA_PKCS          0x00000060UL
#define CKM_SHA3_384_RSA_PKCS          0x00000061UL
#define CKM_SHA3_512_RSA_PKCS          0x00000062UL
#define CKM_SHA3_256_RSA_PKCS_PSS      0x00000063UL
#define CKM_SHA3_384_RSA_PKCS_PSS      0x00000064UL
#define CKM_SHA3_512_RSA_PKCS_PSS      0x00000065UL
#define CKM_SHA3_224_RSA_PKCS          0x00000066UL
#define CKM_SHA3_224_RSA_PKCS_PSS      0x00000067UL

#define CKM_BLAKE2B_160                0x0000400cUL
#define CKM_BLAKE2B_160_HMAC           0x0000400dUL
#define CKM_BLAKE2B_160_HMAC_GENERAL   0x0000400eUL
#define CKM_BLAKE2B_160_KEY_DERIVE     0x0000400fUL
#define CKM_BLAKE2B_160_KEY_GEN        0x00004010UL
#define CKM_BLAKE2B_256                0x00004011UL
#define CKM_BLAKE2B_256_HMAC           0x00004012UL
#define CKM_BLAKE2B_256_HMAC_GENERAL   0x00004013UL
#define CKM_BLAKE2B_256_KEY_DERIVE     0x00004014UL
#define CKM_BLAKE2B_256_KEY_GEN        0x00004015UL
#define CKM_BLAKE2B_384                0x00004016UL
#define CKM_BLAKE2B_384_HMAC           0x00004017UL
#define CKM_BLAKE2B_384_HMAC_GENERAL   0x00004018UL
#define CKM_BLAKE2B_384_KEY_DERIVE     0x00004019UL
#define CKM_BLAKE2B_384_KEY_GEN        0x0000401aUL
#define CKM_BLAKE2B_512                0x0000401bUL
#define CKM_BLAKE2B_512_HMAC           0x0000401cUL
#define CKM_BLAKE2B_512_HMAC_GENERAL   0x0000401dUL
#define CKM_BLAKE2B_512_KEY_DERIVE     0x0000401eUL
#define CKM_BLAKE2B_512_KEY_GEN        0x0000401fUL

#endif
