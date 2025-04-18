# Building PKCS#11 library
Due to the mutual incompatibility of some Linux distributions, it will be necessary to make a separate build `BouncyHsm.Pkcs11Lib`.

## Prerequisites
- _clang_ compiler or _gcc_
- _make_
- _git_


The compiled library is named `BouncyHsm.Pkcs11Lib-x64.so`.

## Makefile parameters
To facilitate building for other platforms (ARM, MAC OS), it is possible to parameterize the makefile:
- `CC` - for compiler, default is `clang`,
- `ARCH_FLAGS` - for architcture, default is `-m64`,
- `EXTRA_FLAGS` - for extra compilation flags.

Example build command for _ARM_ with _gcc_:
```
make CC=gcc ARCH_FLAGS="-march=armv8-a"
```

## Example compilation for Alpine Linux
Install prerequisites:

```
apk add build-base
apk add git
```

Clone repository:
```
git clone https://github.com/harrison314/BouncyHsm.git
```

Build:
```
cd BouncyHsm
# git checkout <last tag>
cd build_linux
make CC=gcc
```

## Example compilation for Rocky Linux 9/AlmaLinux OS 9
The native library for _AlmaLinux 9_ (RHEL like distributions) is already connected to BouncyHsm, the following procedure serves as a demonstration.

Install prerequisites:

```
dnf install make gcc clang git -y
```

Clone repository:
```
git clone https://github.com/harrison314/BouncyHsm.git
```

Build:
```
cd BouncyHsm
# git checkout <last tag>
cd build_linux
make
```

The compiled library is named `BouncyHsm.Pkcs11Lib-x64.so`.