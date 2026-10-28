FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /App

USER $APP_UID 
COPY --chown=$APP_UID:$APP_UID /artifacts/BouncyHsm /App
COPY --chown=$APP_UID:$APP_UID --chmod=u=rwx,g=rx /artifacts/BouncyHsm/data /var/BouncyHsm

EXPOSE 8080
EXPOSE 8765

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080 

ENTRYPOINT ["dotnet", "BouncyHsm.dll"] 
