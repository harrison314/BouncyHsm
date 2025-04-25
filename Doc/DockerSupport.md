# Docker support
_Bouncy Hsm_ does not publish a Docker image, but it is ready to use with Docker, both the server and the PKCS#11 library.

## Dockerize server
The Docker image for the server can be created using the following Dockerfile:

```dockerfile
FROM alpine:3.20.3

ENV APP_VERSION=1.4.0

WORKDIR /unzip
ADD https://github.com/harrison314/BouncyHsm/releases/download/v${APP_VERSION}/BouncyHsm.zip .
RUN apk --update add unzip && rm -rf /var/cache/apk/* && unzip BouncyHsm.zip && rm BouncyHsm.zip

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App

COPY --from=0 /unzip /App

EXPOSE 8080
EXPOSE 8765
VOLUME /var/BouncyHsm/

ENV ASPNETCORE_ENVIRONMENT=Docker

CMD ["dotnet", "BouncyHsm.dll"]
```

Additional configuration is possible using environment variables with the `BouncyHsm_` prefix.
The way to override the default configuration is described on the [.NET framework pages](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#non-prefixed-environment-variables).

Some useful environmental variables:
- `BouncyHsm_Serilog__MinimumLevel__Default=Debug` - set default log level (possible values are: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`),
- `BouncyHsm_Serilog__Properties__<name>=<value>` - set log property with _name_ and _value_,
- `BouncyHsm_BouncyHsmSetup__ProtectedAuthPathTimeout=00:05:00` - set timout for protected authorization path.

For more complicated behavior modifications or logging modifications,
I recommend copying your own `appsetings.json` configuration file into the image and removing the `ENV ASPNETCORE_ENVIRONMENT=Docker` line from the Dockerfile.


## Dockerize clients with PKCS#11 library
When dockerizing applications that use the native PKCS#11 library (_BouncyHsm.Pkcs11Lib.dll_, _BouncyHsm.Pkcs11Lib.so_),
the `BOUNCY_HSM_CFG_STRING` environment variable is set in the container, which the library uses to find a TCP endpoint to connect to.

For example, for a server on the *bouncy_hsm_server* domain, the `BOUNCY_HSM_CFG_STRING` variable is set as follows: `Server=bouncy_hsm_server; Port=8765;`,
to add logging to the console, use: `Server=bouncy_hsm_server; Port=8765; LogTarget=Console; LogLevel=TRACE;`.

The value of the environment variable `BOUNCY_HSM_CFG_STRING` can be called in the Bouncy Hsm web interface under _Configure PKCS#11 lib_.


## Docker compose example
In folder [/Examples/DockerComposeExample](/Examples/DockerComposeExample) there is a demonstration of using Docker compose.
In the first container is _Bouncy Hsm_ server, whose dockerfile is described above.
In the second, an application that lists the supported mechanisms via the PKCS#11 library.

In the second container, two environment variables are set:
- `BOUNCY_HSM_CFG_STRING` - connection string for PKCS#11 library,
- `BOUNCY_HSM_HTTP` - HTTP _Bouncy Hsm_ endpoint for application.

```yaml
services:
  bouncy_hsm_server:
    build: BouncyHsmServer
    ports:
      - "8080:8080"
      - "8765:8765"
  test_service:
    build: TestContainer
    environment:
      BOUNCY_HSM_CFG_STRING: "Server=bouncy_hsm_server; Port=8765;"
      BOUNCY_HSM_HTTP: "http://bouncy_hsm_server:8080/"
    depends_on:
      - bouncy_hsm_server
```

To run the example, use the command: `docker compose up`.

_TestContainer_ uses the PKCS#11 library  _BouncyHsm.Pkcs11Lib.so_, which is obtained from the nuget package BouncyHsm.Client, for other applications the library must be obtained in other ways.

For example - download from release:

```dockerfile
FROM alpine:3.20.3

ENV APP_VERSION=1.4.0

WORKDIR /unzip
ADD https://github.com/harrison314/BouncyHsm/releases/download/v${APP_VERSION}/BouncyHsm.zip .
RUN apk --update add unzip && rm -rf /var/cache/apk/* && unzip BouncyHsm.zip && rm BouncyHsm.zip

FROM debian
WORKDIR /App

COPY --from=0 /unzip/native/Linux-x64 /App/NativeLibLocation

CMD ["example_application", "-p11libPath", "/App/NativeLibLocation/BouncyHsm.Pkcs11Lib.so"]
```
