﻿# Supported algorithms

Supported algorithms for _Bouncy Hsm_ version 1.5.0.0 (commit _2b790ddc35d303523962a2d8db3204ec2f23a408_).

## Mechanisms
_Bouncy Hsm_ supports 166 mechanisms.

Supported mechanisms are by default for PKCS#11 specification [version 2.40](https://docs.oasis-open.org/pkcs11/pkcs11-curr/v2.40/os/pkcs11-curr-v2.40-os.pdf),
and native APIs are also used for it.
_Bouncy&nbsp;Hsm_ allows the use of some mechanisms from PKCS#11 specification [version 3.0](https://docs.oasis-open.org/pkcs11/pkcs11-curr/v3.0/pkcs11-curr-v3.0.pdf).

| Mechanism | Min key size | Max key size | Digest | Sign, Verify | SignRecover, VerifyRecover | Derive | Encrypt, Decrypt | Generate key pair | Generate key | Wrap, Unwrap |
| :---  | ---: | ---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| `CKM_RSA_PKCS_KEY_PAIR_GEN`  | 2048  | 6144 |   |   |   |   |   | ✓ |   |   |
| `CKM_RSA_PKCS`  | 2048  | 6144 |   | ✓ | ✓ |   | ✓ |   |   | ✓ |
| `CKM_RSA_9796`  | 2048  | 6144 |   | ✓ | ✓ |   |   |   |   |   |
| `CKM_MD2_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_MD5_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA1_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_RIPEMD128_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_RIPEMD160_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_RSA_PKCS_OAEP`  | 2048  | 6144 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_RSA_X9_31_KEY_PAIR_GEN`  | 2048  | 6144 |   |   |   |   |   | ✓ |   |   |
| `CKM_SHA1_RSA_X9_31`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_RSA_PKCS_PSS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA1_RSA_PKCS_PSS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA256_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA384_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA256_RSA_PKCS_PSS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA384_RSA_PKCS_PSS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_RSA_PKCS_PSS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA224_RSA_PKCS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA224_RSA_PKCS_PSS`  | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_224`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA512_224_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_224_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_224_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA512_256`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA512_256_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_256_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_256_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA512_T`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA3_256_RSA_PKCS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_384_RSA_PKCS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_512_RSA_PKCS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_256_RSA_PKCS_PSS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_384_RSA_PKCS_PSS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_512_RSA_PKCS_PSS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_224_RSA_PKCS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_224_RSA_PKCS_PSS` <sub>v3.0</sub> | 2048  | 6144 |   | ✓ |   |   |   |   |   |   |
| `CKM_MD2`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_MD2_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_MD2_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_MD5`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_MD5_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_MD5_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA_1`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA_1_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA_1_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_RIPEMD128`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_RIPEMD128_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_RIPEMD128_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_RIPEMD160`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_RIPEMD160_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_RIPEMD160_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA256`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA256_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA256_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA224`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA224_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA224_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA384`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA384_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA384_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA512_HMAC`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA512_HMAC_GENERAL`  | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_256` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA3_256_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_256_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_256_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA3_224` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA3_224_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_224_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_224_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA3_384` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA3_384_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_384_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_384_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA3_512` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_SHA3_512_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_512_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_SHA3_512_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_GENERIC_SECRET_KEY_GEN`  | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_CONCATENATE_BASE_AND_KEY`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_CONCATENATE_BASE_AND_DATA`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_CONCATENATE_DATA_AND_BASE`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_XOR_BASE_AND_DATA`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_EXTRACT_KEY_FROM_KEY`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_MD5_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_MD2_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA1_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA256_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA384_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA512_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA224_KEY_DERIVATION`  | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA3_256_KEY_DERIVATION` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA3_224_KEY_DERIVATION` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA3_384_KEY_DERIVATION` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_SHA3_512_KEY_DERIVATION` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_ECDSA_KEY_PAIR_GEN`  | 192  | 521 |   |   |   |   |   | ✓ |   |   |
| `CKM_ECDSA`  | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA1`  | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA224`  | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA256`  | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA384`  | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA512`  | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA3_224` <sub>v3.0</sub> | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA3_256` <sub>v3.0</sub> | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA3_384` <sub>v3.0</sub> | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDSA_SHA3_512` <sub>v3.0</sub> | 192  | 521 |   | ✓ |   |   |   |   |   |   |
| `CKM_ECDH1_DERIVE`  | 192  | 521 |   |   |   | ✓ |   |   |   |   |
| `CKM_ECDH1_COFACTOR_DERIVE`  | 192  | 521 |   |   |   | ✓ |   |   |   |   |
| `CKM_EC_EDWARDS_KEY_PAIR_GEN` <sub>v3.0</sub> | 32  | 57 |   |   |   |   |   | ✓ |   |   |
| `CKM_EC_MONTGOMERY_KEY_PAIR_GEN` <sub>v3.0</sub> | 32  | 57 |   |   |   |   |   | ✓ |   |   |
| `CKM_EDDSA` <sub>v3.0</sub> | 32  | 57 |   | ✓ |   |   |   |   |   |   |
| `CKM_AES_KEY_GEN`  | 16  | 32 |   |   |   |   |   |   | ✓ |   |
| `CKM_AES_ECB`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CBC`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CBC_PAD`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CTR`  | 16  | 32 |   |   |   |   | ✓ |   |   |   |
| `CKM_AES_GCM`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CCM`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CTS`  | 16  | 32 |   |   |   |   | ✓ |   |   |   |
| `CKM_AES_ECB_ENCRYPT_DATA`  | 16  | 32 |   |   |   | ✓ |   |   |   |   |
| `CKM_AES_CBC_ENCRYPT_DATA`  | 16  | 32 |   |   |   | ✓ |   |   |   |   |
| `CKM_GOSTR3411`  | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_CHACHA20_KEY_GEN` <sub>v3.0</sub> | 32  | 32 |   |   |   |   |   |   | ✓ |   |
| `CKM_CHACHA20` <sub>v3.0</sub> | 32  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_POLY1305_KEY_GEN` <sub>v3.0</sub> | 32  | 32 |   |   |   |   |   |   | ✓ |   |
| `CKM_POLY1305` <sub>v3.0</sub> | 32  | 32 |   | ✓ |   |   |   |   |   |   |
| `CKM_AES_OFB`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CFB64`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CFB8`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CFB128`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_CFB1`  | 16  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_AES_KEY_WRAP_PAD`  | 16  | 32 |   |   |   |   |   |   |   | ✓ |
| `CKM_SHA_1_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA224_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA256_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA384_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA512_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA512_224_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA512_256_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SHA512_T_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_BLAKE2B_160` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_BLAKE2B_160_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_160_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_160_KEY_DERIVE` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_BLAKE2B_160_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_BLAKE2B_256` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_BLAKE2B_256_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_256_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_256_KEY_DERIVE` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_BLAKE2B_256_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_BLAKE2B_384` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_BLAKE2B_384_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_384_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_384_KEY_DERIVE` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_BLAKE2B_384_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_BLAKE2B_512` <sub>v3.0</sub> | 0  | 0 | ✓ |   |   |   |   |   |   |   |
| `CKM_BLAKE2B_512_HMAC` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_512_HMAC_GENERAL` <sub>v3.0</sub> | 1  | 10485760 |   | ✓ |   |   |   |   |   |   |
| `CKM_BLAKE2B_512_KEY_DERIVE` <sub>v3.0</sub> | 1  | 10485760 |   |   |   | ✓ |   |   |   |   |
| `CKM_BLAKE2B_512_KEY_GEN` <sub>v3.0</sub> | 1  | 10485760 |   |   |   |   |   |   | ✓ |   |
| `CKM_SALSA20` <sub>v3.0</sub> | 32  | 32 |   |   |   |   | ✓ |   |   | ✓ |
| `CKM_CHACHA20_POLY1305` <sub>v3.0</sub> | 32  | 32 |   |   |   |   | ✓ |   |   |   |
| `CKM_SALSA20_KEY_GEN` <sub>v3.0</sub> | 32  | 32 |   |   |   |   |   |   | ✓ |   |

## Elliptic curves
_Bouncy Hsm_ supports 84 different named elliptic curves.

| Kind | Curve | OID |
| ---  | --- | --- |
| X962 | prime192v1 | `1.2.840.10045.3.1.1` |
| X962 | prime192v2 | `1.2.840.10045.3.1.2` |
| X962 | prime192v3 | `1.2.840.10045.3.1.3` |
| X962 | prime239v1 | `1.2.840.10045.3.1.4` |
| X962 | prime239v2 | `1.2.840.10045.3.1.5` |
| X962 | prime239v3 | `1.2.840.10045.3.1.6` |
| X962 | prime256v1 | `1.2.840.10045.3.1.7` |
| X962 | c2pnb163v1 | `1.2.840.10045.3.0.1` |
| X962 | c2pnb163v2 | `1.2.840.10045.3.0.2` |
| X962 | c2pnb163v3 | `1.2.840.10045.3.0.3` |
| X962 | c2pnb176w1 | `1.2.840.10045.3.0.4` |
| X962 | c2tnb191v1 | `1.2.840.10045.3.0.5` |
| X962 | c2tnb191v2 | `1.2.840.10045.3.0.6` |
| X962 | c2tnb191v3 | `1.2.840.10045.3.0.7` |
| X962 | c2pnb208w1 | `1.2.840.10045.3.0.10` |
| X962 | c2tnb239v1 | `1.2.840.10045.3.0.11` |
| X962 | c2tnb239v2 | `1.2.840.10045.3.0.12` |
| X962 | c2tnb239v3 | `1.2.840.10045.3.0.13` |
| X962 | c2pnb272w1 | `1.2.840.10045.3.0.16` |
| X962 | c2pnb304w1 | `1.2.840.10045.3.0.17` |
| X962 | c2tnb359v1 | `1.2.840.10045.3.0.18` |
| X962 | c2pnb368w1 | `1.2.840.10045.3.0.19` |
| X962 | c2tnb431r1 | `1.2.840.10045.3.0.20` |
| SAC | secp112r1 | `1.3.132.0.6` |
| SAC | secp112r2 | `1.3.132.0.7` |
| SAC | secp128r1 | `1.3.132.0.28` |
| SAC | secp128r2 | `1.3.132.0.29` |
| SAC | secp160k1 | `1.3.132.0.9` |
| SAC | secp160r1 | `1.3.132.0.8` |
| SAC | secp160r2 | `1.3.132.0.30` |
| SAC | secp192k1 | `1.3.132.0.31` |
| SAC | secp192r1 | `1.2.840.10045.3.1.1` |
| SAC | secp224k1 | `1.3.132.0.32` |
| SAC | secp224r1 | `1.3.132.0.33` |
| SAC | secp256k1 | `1.3.132.0.10` |
| SAC | secp256r1 | `1.2.840.10045.3.1.7` |
| SAC | secp384r1 | `1.3.132.0.34` |
| SAC | secp521r1 | `1.3.132.0.35` |
| SAC | sect113r1 | `1.3.132.0.4` |
| SAC | sect113r2 | `1.3.132.0.5` |
| SAC | sect131r1 | `1.3.132.0.22` |
| SAC | sect131r2 | `1.3.132.0.23` |
| SAC | sect163k1 | `1.3.132.0.1` |
| SAC | sect163r1 | `1.3.132.0.2` |
| SAC | sect163r2 | `1.3.132.0.15` |
| SAC | sect193r1 | `1.3.132.0.24` |
| SAC | sect193r2 | `1.3.132.0.25` |
| SAC | sect233k1 | `1.3.132.0.26` |
| SAC | sect233r1 | `1.3.132.0.27` |
| SAC | sect239k1 | `1.3.132.0.3` |
| SAC | sect283k1 | `1.3.132.0.16` |
| SAC | sect283r1 | `1.3.132.0.17` |
| SAC | sect409k1 | `1.3.132.0.36` |
| SAC | sect409r1 | `1.3.132.0.37` |
| SAC | sect571k1 | `1.3.132.0.38` |
| SAC | sect571r1 | `1.3.132.0.39` |
| NIST | B-163 | `1.3.132.0.15` |
| NIST | B-233 | `1.3.132.0.27` |
| NIST | B-283 | `1.3.132.0.17` |
| NIST | B-409 | `1.3.132.0.37` |
| NIST | B-571 | `1.3.132.0.39` |
| NIST | K-163 | `1.3.132.0.1` |
| NIST | K-233 | `1.3.132.0.26` |
| NIST | K-283 | `1.3.132.0.16` |
| NIST | K-409 | `1.3.132.0.36` |
| NIST | K-571 | `1.3.132.0.38` |
| NIST | P-192 | `1.2.840.10045.3.1.1` |
| NIST | P-224 | `1.3.132.0.33` |
| NIST | P-256 | `1.2.840.10045.3.1.7` |
| NIST | P-384 | `1.3.132.0.34` |
| NIST | P-521 | `1.3.132.0.35` |
| TeleTrusT | brainpoolP160r1 | `1.3.36.3.3.2.8.1.1.1` |
| TeleTrusT | brainpoolP160t1 | `1.3.36.3.3.2.8.1.1.2` |
| TeleTrusT | brainpoolP192r1 | `1.3.36.3.3.2.8.1.1.3` |
| TeleTrusT | brainpoolP192t1 | `1.3.36.3.3.2.8.1.1.4` |
| TeleTrusT | brainpoolP224r1 | `1.3.36.3.3.2.8.1.1.5` |
| TeleTrusT | brainpoolP224t1 | `1.3.36.3.3.2.8.1.1.6` |
| TeleTrusT | brainpoolP256r1 | `1.3.36.3.3.2.8.1.1.7` |
| TeleTrusT | brainpoolP256t1 | `1.3.36.3.3.2.8.1.1.8` |
| TeleTrusT | brainpoolP320r1 | `1.3.36.3.3.2.8.1.1.9` |
| TeleTrusT | brainpoolP320t1 | `1.3.36.3.3.2.8.1.1.10` |
| TeleTrusT | brainpoolP384r1 | `1.3.36.3.3.2.8.1.1.11` |
| TeleTrusT | brainpoolP384t1 | `1.3.36.3.3.2.8.1.1.12` |
| TeleTrusT | brainpoolP512r1 | `1.3.36.3.3.2.8.1.1.13` |
| TeleTrusT | brainpoolP512t1 | `1.3.36.3.3.2.8.1.1.14` |
| Ancii | FRP256v1 | `1.2.250.1.223.101.256.1` |
| ECGost3410 | GostR3410-2001-CryptoPro-A | `1.2.643.2.2.35.1` |
| ECGost3410 | GostR3410-2001-CryptoPro-B | `1.2.643.2.2.35.2` |
| ECGost3410 | GostR3410-2001-CryptoPro-C | `1.2.643.2.2.35.3` |
| ECGost3410 | GostR3410-2001-CryptoPro-XchA | `1.2.643.2.2.36.0` |
| ECGost3410 | GostR3410-2001-CryptoPro-XchB | `1.2.643.2.2.36.1` |
| ECGost3410 | Tc26-Gost-3410-12-256-paramSetA | `1.2.643.7.1.2.1.1.1` |
| ECGost3410 | Tc26-Gost-3410-12-256-paramSetB | `1.2.643.7.1.2.1.1.2` |
| ECGost3410 | Tc26-Gost-3410-12-256-paramSetC | `1.2.643.7.1.2.1.1.3` |
| ECGost3410 | Tc26-Gost-3410-12-256-paramSetD | `1.2.643.7.1.2.1.1.4` |
| ECGost3410 | Tc26-Gost-3410-12-512-paramSetA | `1.2.643.7.1.2.1.2.1` |
| ECGost3410 | Tc26-Gost-3410-12-512-paramSetB | `1.2.643.7.1.2.1.2.2` |
| ECGost3410 | Tc26-Gost-3410-12-512-paramSetC | `1.2.643.7.1.2.1.2.3` |
| GMN | wapip192v1 | `1.2.156.10197.1.301.101` |
| GMN | wapi192v1 | `1.2.156.11235.1.1.1` |
| GMN | sm2p256v1 | `1.2.156.10197.1.301` |

## Edwards curves
_Bouncy Hsm_ supports 2 different named edwards curves.

| Kind | Curve | Curve Name | OID |
| ---  | --- | --- | --- |
| Edwards | Ed25519 | `id-Ed25519` | `1.3.101.112` |
| Edwards | Ed448 | `id-Ed448` | `1.3.101.113` |

## Montgomery curves
_Bouncy Hsm_ supports 2 different named montgomery curves.

| Kind | Curve | Curve Name | OID |
| ---  | --- | --- | --- |
| Montgomery | X25519 | `id-X25519` | `1.3.101.110` |
| Montgomery | X448 | `id-X448` | `1.3.101.111` |
