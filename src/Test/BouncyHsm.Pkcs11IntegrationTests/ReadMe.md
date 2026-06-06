## How to run unit test

1. Build native library
    1. On Windows using Vusial Studio
    2. On Debian/Ubuntu/AlmaLinux use makefile
2. Run Bouncy Hsm on localhost.
3. Create empty slot if not exists with following parameters:

```json
{
  "IsHwDevice": true,
  "IsRemovableDevice": false,
  "Description": "TestDevice1",
  "Token": {
    "Label": "TestToken1",
    "SerialNumber": null,
    "SimulateHwRng": true,
    "SimulateHwMechanism": true,
    "SimulateQualifiedArea": false,
    "SimulateProtectedAuthPath": false,
    "SpeedMode": "WithoutRestriction",
    "UserPin": "123456",
    "SoPin": "12345678",
    "SignaturePin": null
  }
}
```
or using BouncyHsm.Cli:
```
dotnet BouncyHsm.Cli.dll slot create -d "TestDevice1" -l "TestToken1" -u 123456 -q 12345678 -e http://localhost:5291/ 
```

4. Run tests using:
```
  dotnet clean
  dotnet test
```

Or run without issue tests:
```
dotnet test --filter "TestCategory!=IssueTest"
```