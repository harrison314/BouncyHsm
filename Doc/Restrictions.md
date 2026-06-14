# Restrictions against the PKCS#11 standard
List of known restrictions and known issues regarding the PKCS#11 standard for individual versions.

## Restrictions
Here is a list of general restrictions and deviations of BouncyHsm that were architectural decisions:
- PKCS#11 functions return `CKR_DEVICE_ERROR` when there is a communication error between the native library and the server (for example, unavailability, or an error in the TCP stack).
- When using template attributes (`CK_ATTRIBUTE[]`), only nesting at level 10 is supported (via `CKA_DERIVE_TEMPLATE`, `CKA_WRAP_TEMPLATE` and `CKA_UNWRAP_TEMPLATE`).

### v2.2.0
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

#### CK_SP800_108_KDF_PARAMS
- Does not accept Additional Derived Keys (`ulAdditionalDerivedKeys` must by 0 and `pAdditionalDerivedKeys` NULL).
- `CK_SP800_108_COUNTER_FORMAT` for `CKM_SP800_108_COUNTER_KDF` and `CK_SP800_108_ITERATION_VARIABLE` only accepts values 8, 16, 24, 32, 40, 48, 56, 64.
- `CK_SP800_108_COUNTER_FORMAT` for `CKM_SP800_108_DOUBLE_PIPELINE_KDF` and `CK_SP800_108_COUNTER` only accepts values 8, 16, 24, 32, 40, 48, 56, 64.
- `CK_SP800_108_COUNTER_FORMAT` for `CKM_SP800_108_DOUBLE_PIPELINE_KDF` and `CK_SP800_108_ITERATION_VARIABLE` only accepts multiples of octal.

#### CK_SP800_108_FEEDBACK_KDF_PARAMS
- Does not accept Additional Derived Keys (`ulAdditionalDerivedKeys` must by 0 and `pAdditionalDerivedKeys` NULL).
- `CK_SP800_108_COUNTER_FORMAT` for `CKM_SP800_108_FEEDBACK_KDF` and `CK_SP800_108_COUNTER` only accepts values 8, 16, 24, 32, 40, 48, 56, 64.
- `CK_SP800_108_COUNTER_FORMAT` for `CKM_SP800_108_FEEDBACK_KDF` and `CK_SP800_108_ITERATION_VARIABLE` only accepts multiples of octal.

---

### v2.1.1
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

---

### v2.1.0
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

---

### v2.0.1
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

---

### v2.0.0
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

---

### v1.6.1
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

---

### v1.6.0
#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.

---

### v1.5.0

#### CK_CHACHA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).

#### CK_SALSA20_PARAMS 
- Accept only value 0 for blockCounter (filed `pBlockCounter`).
- Accept only 64 and 192 bits nonce length.