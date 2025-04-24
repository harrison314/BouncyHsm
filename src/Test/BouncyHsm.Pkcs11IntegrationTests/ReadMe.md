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
    "SpeedMode": "WithoutRestriction",
    "UserPin": "123456",
    "SoPin": "12345678",
    "SignaturePin": null
  }
}
```
or using BouncyHsm.Cli:
```
dotnet BouncyHsm.Cli.dll slot create -e http://localhost:5000/ -d "TestDevice1" -l "TestToken1" -u 123456 -q 12345678
```

3. Run tests using:
```
  dotnet clean
  dotnet test
```
