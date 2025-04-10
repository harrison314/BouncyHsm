FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /App

COPY  BouncyHsmTestExample /App

# For PKCS11Interop lib
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

CMD ["dotnet", "test"]