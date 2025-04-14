FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /App

COPY  ExampleApp /App

# Fix issue with Pkcs11Interop https://github.com/Pkcs11Interop/Pkcs11Interop/issues/212#issuecomment-1236292990
#RUN ln -s /lib64/libdl.so.2 /lib64/libdl.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

RUN dotnet restore & dotnet build
CMD ["dotnet", "run"]