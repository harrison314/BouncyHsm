FROM alpine:3.20.3

ENV APP_VERSION=1.3.0

WORKDIR /unzip
ADD https://github.com/harrison314/BouncyHsm/releases/download/v${APP_VERSION}/BouncyHsm.zip .
RUN apk --update add unzip && rm -rf /var/cache/apk/* && unzip BouncyHsm.zip && rm BouncyHsm.zip

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App

COPY --from=0 /unzip /App

EXPOSE 8080
EXPOSE 8765
VOLUME /App/bin

CMD ["dotnet", "BouncyHsm.dll"]