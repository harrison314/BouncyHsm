## How to run unit test

1. Create empty slot if not exists with following parameters:

```json
{
  "IsHwDevice": true,
  "Description": "TestDevice1",
  "Token": {
    "Label": "TestToken1",
    "SerialNumber": null,
    "SimulateHwRng": true,
    "SimulateHwMechanism": true,
    "SimulateQualifiedArea": false,
    "UserPin": "123456",
    "SoPin": "12345678",
    "SignaturePin": null
  }
}
```

2. Run Bouncy Hsm on localhost.
3. Run tests.