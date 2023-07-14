## How to run unit test

1. Build native library
  1.1 On WIndows using Vusial Studio
  1.2 On Debian/Ubuntu use makefile
2. Run Bouncy Hsm on localhost.
3. Create empty slot if not exists with following parameters:

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

3. Run tests using:
```
  dotnet clean
  dotnet test
```
