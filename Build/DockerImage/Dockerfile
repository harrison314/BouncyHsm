FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App

COPY /artifacts/BouncyHsm /App

EXPOSE 8080
EXPOSE 8765
VOLUME /var/BouncyHsm/

ENV ASPNETCORE_ENVIRONMENT=Docker

CMD ["dotnet", "BouncyHsm.dll"]