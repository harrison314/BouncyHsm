# Supported algorithms

Supported algorithms for _Bouncy Hsm_ verison 0.1.0.0 (commit _-_).

## Mechanisms
_Bouncy Hsm_ supports 75 mechanisms.

| Mechanism | Min key size | Max key size | Digest | Sign | Verify | Derive | Encrypt | Decrypt |
| :---  | ---: | ---: | :---: | :---: | :---: | :---: | :---: | :---: |
| `CKM_RSA_PKCS_KEY_PAIR_GEN` | 2048  | 6144 |   |   |   |   |   |   |
| `CKM_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_MD2_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_MD5_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA1_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_RIPEMD128_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_RIPEMD160_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_RSA_PKCS_PSS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA1_RSA_PKCS_PSS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA256_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA384_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA256_RSA_PKCS_PSS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA384_RSA_PKCS_PSS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_RSA_PKCS_PSS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA224_RSA_PKCS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA224_RSA_PKCS_PSS` | 2048  | 6144 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_224` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA512_224_HMAC` | 28  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_224_HMAC_GENERAL` | 28  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_224_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA512_256` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA512_256_HMAC` | 32  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_256_HMAC_GENERAL` | 32  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_256_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA512_T` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_MD2` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_MD2_HMAC` | 16  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_MD2_HMAC_GENERAL` | 16  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_MD5` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_MD5_HMAC` | 20  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_MD5_HMAC_GENERAL` | 20  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA_1` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA_1_HMAC` | 20  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA_1_HMAC_GENERAL` | 20  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_RIPEMD128` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_RIPEMD128_HMAC` | 16  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_RIPEMD128_HMAC_GENERAL` | 16  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_RIPEMD160` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_RIPEMD160_HMAC` | 20  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_RIPEMD160_HMAC_GENERAL` | 20  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA256` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA256_HMAC` | 32  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA256_HMAC_GENERAL` | 32  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA224` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA224_HMAC` | 28  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA224_HMAC_GENERAL` | 28  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA384` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA384_HMAC` | 48  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA384_HMAC_GENERAL` | 48  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512` | 0  | 0 | ✓ |   |   |   |   |   |
| `CKM_SHA512_HMAC` | 64  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_SHA512_HMAC_GENERAL` | 64  | 10485760 |   | ✓ | ✓ |   |   |   |
| `CKM_GENERIC_SECRET_KEY_GEN` | 1  | 10485760 |   |   |   |   |   |   |
| `CKM_CONCATENATE_BASE_AND_KEY` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_CONCATENATE_BASE_AND_DATA` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_CONCATENATE_DATA_AND_BASE` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_XOR_BASE_AND_DATA` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_EXTRACT_KEY_FROM_KEY` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_MD5_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_MD2_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA1_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA256_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA384_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA512_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_SHA224_KEY_DERIVATION` | 1  | 10485760 |   |   |   | ✓ |   |   |
| `CKM_ECDSA_KEY_PAIR_GEN` | 192  | 521 |   |   |   |   |   |   |
| `CKM_ECDSA` | 192  | 521 |   | ✓ | ✓ |   |   |   |
| `CKM_ECDSA_SHA1` | 192  | 521 |   | ✓ | ✓ |   |   |   |
| `CKM_ECDSA_SHA224` | 192  | 521 |   | ✓ | ✓ |   |   |   |
| `CKM_ECDSA_SHA256` | 192  | 521 |   | ✓ | ✓ |   |   |   |
| `CKM_ECDSA_SHA384` | 192  | 521 |   | ✓ | ✓ |   |   |   |
| `CKM_ECDSA_SHA512` | 192  | 521 |   | ✓ | ✓ |   |   |   |
| `CKM_ECDH1_DERIVE` | 192  | 521 |   |   |   | ✓ |   |   |
| `CKM_GOSTR3411` | 0  | 0 | ✓ |   |   |   |   |   |

## Elitic curves
_Bouncy Hsm_ supports 80 different named elliptic curves.

| Curve | Kind | OID |
| ---  | --- | --- |
| prime192v1 | X962 | `1.2.840.10045.3.1.1` |
| prime192v2 | X962 | `1.2.840.10045.3.1.2` |
| prime192v3 | X962 | `1.2.840.10045.3.1.3` |
| prime239v1 | X962 | `1.2.840.10045.3.1.4` |
| prime239v2 | X962 | `1.2.840.10045.3.1.5` |
| prime239v3 | X962 | `1.2.840.10045.3.1.6` |
| prime256v1 | X962 | `1.2.840.10045.3.1.7` |
| c2pnb163v1 | X962 | `1.2.840.10045.3.0.1` |
| c2pnb163v2 | X962 | `1.2.840.10045.3.0.2` |
| c2pnb163v3 | X962 | `1.2.840.10045.3.0.3` |
| c2pnb176w1 | X962 | `1.2.840.10045.3.0.4` |
| c2tnb191v1 | X962 | `1.2.840.10045.3.0.5` |
| c2tnb191v2 | X962 | `1.2.840.10045.3.0.6` |
| c2tnb191v3 | X962 | `1.2.840.10045.3.0.7` |
| c2pnb208w1 | X962 | `1.2.840.10045.3.0.10` |
| c2tnb239v1 | X962 | `1.2.840.10045.3.0.11` |
| c2tnb239v2 | X962 | `1.2.840.10045.3.0.12` |
| c2tnb239v3 | X962 | `1.2.840.10045.3.0.13` |
| c2pnb272w1 | X962 | `1.2.840.10045.3.0.16` |
| c2pnb304w1 | X962 | `1.2.840.10045.3.0.17` |
| c2tnb359v1 | X962 | `1.2.840.10045.3.0.18` |
| c2pnb368w1 | X962 | `1.2.840.10045.3.0.19` |
| c2tnb431r1 | X962 | `1.2.840.10045.3.0.20` |
| secp112r1 | SAC | `1.3.132.0.6` |
| secp112r2 | SAC | `1.3.132.0.7` |
| secp128r1 | SAC | `1.3.132.0.28` |
| secp128r2 | SAC | `1.3.132.0.29` |
| secp160k1 | SAC | `1.3.132.0.9` |
| secp160r1 | SAC | `1.3.132.0.8` |
| secp160r2 | SAC | `1.3.132.0.30` |
| secp192k1 | SAC | `1.3.132.0.31` |
| secp192r1 | SAC | `1.2.840.10045.3.1.1` |
| secp224k1 | SAC | `1.3.132.0.32` |
| secp224r1 | SAC | `1.3.132.0.33` |
| secp256k1 | SAC | `1.3.132.0.10` |
| secp256r1 | SAC | `1.2.840.10045.3.1.7` |
| secp384r1 | SAC | `1.3.132.0.34` |
| secp521r1 | SAC | `1.3.132.0.35` |
| sect113r1 | SAC | `1.3.132.0.4` |
| sect113r2 | SAC | `1.3.132.0.5` |
| sect131r1 | SAC | `1.3.132.0.22` |
| sect131r2 | SAC | `1.3.132.0.23` |
| sect163k1 | SAC | `1.3.132.0.1` |
| sect163r1 | SAC | `1.3.132.0.2` |
| sect163r2 | SAC | `1.3.132.0.15` |
| sect193r1 | SAC | `1.3.132.0.24` |
| sect193r2 | SAC | `1.3.132.0.25` |
| sect233k1 | SAC | `1.3.132.0.26` |
| sect233r1 | SAC | `1.3.132.0.27` |
| sect239k1 | SAC | `1.3.132.0.3` |
| sect283k1 | SAC | `1.3.132.0.16` |
| sect283r1 | SAC | `1.3.132.0.17` |
| sect409k1 | SAC | `1.3.132.0.36` |
| sect409r1 | SAC | `1.3.132.0.37` |
| sect571k1 | SAC | `1.3.132.0.38` |
| sect571r1 | SAC | `1.3.132.0.39` |
| B-163 | NIST | `1.3.132.0.15` |
| B-233 | NIST | `1.3.132.0.27` |
| B-283 | NIST | `1.3.132.0.17` |
| B-409 | NIST | `1.3.132.0.37` |
| B-571 | NIST | `1.3.132.0.39` |
| K-163 | NIST | `1.3.132.0.1` |
| K-233 | NIST | `1.3.132.0.26` |
| K-283 | NIST | `1.3.132.0.16` |
| K-409 | NIST | `1.3.132.0.36` |
| K-571 | NIST | `1.3.132.0.38` |
| P-192 | NIST | `1.2.840.10045.3.1.1` |
| P-224 | NIST | `1.3.132.0.33` |
| P-256 | NIST | `1.2.840.10045.3.1.7` |
| P-384 | NIST | `1.3.132.0.34` |
| P-521 | NIST | `1.3.132.0.35` |
| brainpoolP160r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.1` |
| brainpoolP160t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.2` |
| brainpoolP192r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.3` |
| brainpoolP192t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.4` |
| brainpoolP224r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.5` |
| brainpoolP224t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.6` |
| brainpoolP256r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.7` |
| brainpoolP256t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.8` |
| brainpoolP320r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.9` |
| brainpoolP320t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.10` |
| brainpoolP384r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.11` |
| brainpoolP384t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.12` |
| brainpoolP512r1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.13` |
| brainpoolP512t1 | TeleTrusT | `1.3.36.3.3.2.8.1.1.14` |
| FRP256v1 | Ancii | `1.2.250.1.223.101.256.1` |
| GostR3410-2001-CryptoPro-A | ECGost3410 | `1.2.643.2.2.35.1` |
| GostR3410-2001-CryptoPro-B | ECGost3410 | `1.2.643.2.2.35.2` |
| GostR3410-2001-CryptoPro-C | ECGost3410 | `1.2.643.2.2.35.3` |
| GostR3410-2001-CryptoPro-XchA | ECGost3410 | `1.2.643.2.2.36.0` |
| GostR3410-2001-CryptoPro-XchB | ECGost3410 | `1.2.643.2.2.36.1` |
| Tc26-Gost-3410-12-256-paramSetA | ECGost3410 | `1.2.643.7.1.2.1.1.1` |
| Tc26-Gost-3410-12-512-paramSetA | ECGost3410 | `1.2.643.7.1.2.1.2.1` |
| Tc26-Gost-3410-12-512-paramSetB | ECGost3410 | `1.2.643.7.1.2.1.2.2` |
| Tc26-Gost-3410-12-512-paramSetC | ECGost3410 | `1.2.643.7.1.2.1.2.3` |
| wapip192v1 | GMN | `1.2.156.10197.1.301.101` |
| sm2p256v1 | GMN | `1.2.156.10197.1.301` |

