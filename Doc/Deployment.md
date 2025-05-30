# Deployment as service

This document describes the deployment for the supported platforms.
_Bouncy Hsm_ can also be used by just running it under the current user (non-privileged user).

## Prerequisites
- [Net 8.0 Runtime and ASP.NET Core Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Deploy as Windows service
_Bouncy Hsm_ can be deployed as a Windows service. All the following commands are executed using _PowerShell_ and privileged user.

For more information see <https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-8.0&tabs=netcore-cli>.

We will install the application in the directory `D:\BouncyHsm` (for example).

### Extract application
We will create directories:
- `D:\BouncyHsm`
- `D:\BouncyHsm\bin`
- `D:\BouncyHsm\Logs`
- `D:\BouncyHsm\Data`

Extract `BouncyHsm.zip` into `D:\BouncyHsm\bin`.

Configure `appsettings.json`:
- set `LiteDbPersistentRepositorySetup::DbFilePath` to `D:/BouncyHsm/Data/BouncyHsm.db`
- set file logging in `Serilog::WriteTo::Args::path` to `D:/BouncyHsm/Logs/BouncyHsm.log.txt` (for configure logs see <https://github.com/serilog/serilog-settings-configuration>)
- set web interface endpoint in `Kestrel::Endpoints::Http::Url`
- set basePath for deploy URL if need in `AppBasePath` - Set base path for WebUi and REST API, it must be changed here in the configuration and in `wwwroot/index.html` in the tag `<base href="/" />` for example `"AppBasePath": "/foo"`, `<base href="/foo/" />`

### Create service account

```powershell
New-LocalUser -Name BouncyHsm
```

Provide a strong password when prompted.

Update security policy:

1. Open the _Local Security Policy editor_ by running `secpol.msc`.
1. Expand the Local Policies node and select _User Rights Assignment_.
1. Open the _Log on as a service policy_.
1. Select _Add User or Group_.
1. Provide the object name (user account) using either of the following approaches:
    1. Type the user account `BouncyHsm` in the object name field and select _OK_ to add the user to the policy.
    1. Select _Advanced_. Select _Find Now_. Select the user account from the list. Select _OK_. Select _OK_ again to add the user to the policy.
    1. Select _OK_ or _Apply_ to accept the changes.

### Create a service
Add access for directory `D:\BouncyHsm` for new user.

Set ACL:

```powershell
$cn = Get-WmiObject -Namespace root\cimv2 -Class Win32_ComputerSystem | Select Name
$user = "{0}\BouncyHsm" -f $cn.Name
$acl = Get-Acl "D:\BouncyHsm"
$aclRuleArgs = $user, "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "D:\BouncyHsm"
```
Error _Exception calling "SetAccessRule" with "1" argument(s): "No flags can be set._ can by ignored.

Create a service:

```powershell
$cn = Get-WmiObject -Namespace root\cimv2 -Class Win32_ComputerSystem | Select Name
$user = "{0}\BouncyHsm" -f $cn.Name

New-Service -Name BouncyHsm -BinaryPathName "D:\BouncyHsm\bin\BouncyHsm.exe --contentRoot D:\BouncyHsm\bin" -Credential $user  -Description "Bouncy Hsm instance 1" -DisplayName "Bouncy Hsm" -StartupType Automatic
```

Or see <https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/sc-create?source=recommendations>.

Start service:
```powershell
Start-Service -Name BouncyHsm
``` 

### Check service status
```powershell
Get-Service -Name BouncyHsm
```

## Remove a service and user
Stop services:
```powershell
Stop-Service -Name BouncyHsm
```

Remove a service in Powershell 6:
```powershell
Remove-Service -Name BouncyHsm
```
Remove a service in older Powershell:
```powershell
sc.exe delete BouncyHsm
```

Remove a user:
```powershell
Remove-LocalUser -Name "BouncyHsm"
```

Another option is to use `NT Service` account for service.


## Deploy as Linux daemon
We will install the application in the directory `/opt/BouncyHsm` (for example).

### Extract application
We will create directories:
- `/opt/BouncyHsm`
- `/opt/BouncyHsm/bin`
- `/opt/BouncyHsm/Logs`
- `/opt/BouncyHsm/Data`

Extract `BouncyHsm.zip` into `/opt/BouncyHsm/bin`.

Configure `appsettings.json`:
- set `LiteDbPersistentRepositorySetup::DbFilePath` to `/opt/BouncyHsm/Data/BouncyHsm.db`
- uncomment section in `Serilog::WriteTo` with file logging (`"Name": "File"`)
- set file logging in `Serilog::WriteTo::Args::path` to `/opt/BouncyHsm/Logs/BouncyHsm.log.txt` (for configure logs see <https://github.com/serilog/serilog-settings-configuration>)
- set web interface endpoint in `Kestrel::Endpoints::Http::Url`
- set basePath for deploy URL if need in `AppBasePath` - Set base path for WebUi and REST API, it must be changed here in the configuration and in `wwwroot/index.html` in the tag `<base href="/" />` for example `"AppBasePath": "/foo"`, `<base href="/foo/" />`

### Create daemon user
```bash
sudo groupadd bouncyhsmuser
sudo adduser --system -g bouncyhsmuser --no-create-home bouncyhsmuser
sudo usermod -s /usr/sbin/nologin bouncyhsmuser
```

### Set filesystem rights
```bash
chown -R bouncyhsmuser:bouncyhsmuser /opt/BouncyHsm/bin
chown -R bouncyhsmuser:bouncyhsmuser /opt/BouncyHsm/Logs
chown -R bouncyhsmuser:bouncyhsmuser /opt/BouncyHsm/Data

find /opt/BouncyHsm/bin -type f -exec chmod u=r,g=r {} \;
find /opt/BouncyHsm/bin -type d -exec chmod u=rwx,g=rx {} \;

chmod u=rwx,g=rx /opt/BouncyHsm/Logs
chmod u=rwx,g=rx /opt/BouncyHsm/Data
```

### Create unit file
Create unit file in `/etc/systemd/system/bouncyhsm.service`.

```
[Unit]
Description=BouncyHsm instance

[Service]
WorkingDirectory=/opt/BouncyHsm/bin
ExecStart=/usr/local/bin/dotnet /opt/BouncyHsm/bin/BouncyHsm.dll

Restart=always
RestartSec=10
KillSignal=SIGINT

SyslogIdentifier=bouncyhsm

User=bouncyhsmuser
Group=bouncyhsmuser

Environment=ASPNETCORE_ENVIRONMENT=Production 

[Install]
WantedBy=multi-user.target
```

Enable service (enable start service automatic):
```bash
sudo systemctl enable bouncyhsm.service
```

Start service:

```bash
sudo systemctl start bouncyhsm
```

### Check service status
```bash
sudo systemctl status bouncyhsm
```

Or view console logs:
```bash
sudo journalctl -fu bouncyhsm
```

For more information see <https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-apache?view=aspnetcore-8.0>.
