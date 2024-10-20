# Mechanism profiles
Profiles are used to disable or enable specific mechanisms, thereby enabling a more faithful simulation of a specific type of HSM and smart cards.

Profile is a JSON file, the path to it is set in the configuration `appsettings.json` in `BouncyHsmSetup::ProfileFilePath`.
When this value is set to `null`, no profile is applied, this is also the default value.

File for profile without restrictions:
```json
{
    "Name": "Null profile", //Name of profile (required)
    "Description": null, //Profile description - string or null (optional)
    "Author": null, // Author of profile - string or null (optional)
    "Operations": [], // List of operations with mechanisms
    "EnabledCurves": null // List of enabled EC curves (by name or OID) - array of strings or null (optional)
}
```

### Operations
Each operation manipulates a list of mechanisms in the order in which they are written. The first operation gets the complete list of mechanisms supported by _BouncyHsm_.

#### Enable
The operation enables the use of the mechanism. It will use already set or default parameters.

```json
{
    "Operation": "Enable",
    "Mechanism": "CKM_SHA_1_HMAC" //Mechanism name (required)
}
```

#### RemoveAll
The operation will delete all mechanisms.

```json
{
    "Operation": "RemoveAll"
}
```

#### Remove
The operation removes a specific mechanism.

```json
{
    "Operation": "Remove",
    "Mechanism": "CKM_MD5" //Mechanism name (required)
}
```

#### Add
The operation adds a mechanism with all its parameters.

`Flags` can have values: CKF_ENCRYPT, CKF_DECRYPT, CKF_DIGEST, CKF_SIGN, CKF_VERIFY, CKF_GENERATE, CKF_GENERATE_KEY_PAIR,CKF_WRAP,CKF_UNWRAP, CKF_DERIVE.

```json
{
    "Operation": "Add",
    "Mechanism": "CKM_RSA_PKCS", //Mechanism name (required)
    "MinKeySize": 2048, // Minimum key size (required)
    "MaxKeySize": 4096, // Maximum key size (required)
    "Flags": ["CKF_SIGN", "CKF_VERIFY", "CKF_ENCRYPT", "CKF_DECRYPT"] //CKF flags (required)
}
```

#### Update
Updates the available mechanism. This method is suitable, for example, for adjusting the allowed key sizes.

`Flags` can have values: CKF_ENCRYPT, CKF_DECRYPT, CKF_DIGEST, CKF_SIGN, CKF_VERIFY, CKF_GENERATE, CKF_GENERATE_KEY_PAIR,CKF_WRAP,CKF_UNWRAP, CKF_DERIVE.

```json
{
    "Operation": "Update",
    "Mechanism": "CKM_RSA_PKCS", //Mechanism name (required)
    "MinKeySize": null, // Minimum key size or null (optional)
    "MaxKeySize": 3072, // Maximum key size or null (optional)
    "Flags": ["CKF_SIGN", "CKF_VERIFY" ] //CKF flags or null (optional)
}
```
#### RemoveUsingRegex
Remove mechanism using (C#) regular expression.

```json
{
    "Operation": "RemoveUsingRegex",
    "RegexPattern": ".*MD5" // Regular expression (required)
}
```

#### EnableUsingRegex
Enables mechanisms based on (C#) regular expression.

```json
{
    "Operation": "EnableUsingRegex",
    "RegexPattern": "RSA|SHA" // Regular expression (required)
}
```

#### FilterFips
The operation remove all non-FIPS mechanisms.

```json
{
    "Operation": "FilterFips"
}
```

### Example profile
Sample profile for Slovenian eID card issued in 2024 - only allows RSA PKCS1 signatures and SHA digests.

```json
{
    "Name": "SVK eID",
    "Description": "Slovak eID card qualified electronic signature.",
    "Author": null,
    "Operations": [
        {
            "Operation": "RemoveAll"
        },
        {
            "Operation": "Add",
            "Mechanism": "CKM_RSA_PKCS",
            "MinKeySize": 4096,
            "MaxKeySize": 4096,
            "Flags": [
                "CKF_SIGN",
                "CKF_VERIFY"
            ]
        },
        {
            "Operation": "Enable",
            "Mechanism": "CKM_SHA_1"
        },
        {
            "Operation": "Enable",
            "Mechanism": "CKM_SHA256"
        },
        {
            "Operation": "Enable",
            "Mechanism": "CKM_SHA384"
        },
        {
            "Operation": "Enable",
            "Mechanism": "CKM_SHA512"
        }
    ],
    "EnabledCurves": []
}
```

Sample profile for FIPS mode.

```json
{
    "Name": "FIPS mode",
    "Description": "Profile for FIPS mode.",
    "Author": null,
    "Operations": [
        {
            "Operation": "FilterFips"
        }
    ],
    "EnabledCurves": [
        "P-192",
        "P-224",
        "P-256",
        "P-384",
        "P-521",
        "B-163",
        "B-283",
        "B-233",
        "B-409",
        "B-571",
        "K-163",
        "K-233",
        "K-283",
        "K-409",
        "K-571"
    ]
}
```
