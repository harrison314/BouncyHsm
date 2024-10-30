# Building PKCS#11 library
Due to the mutual incompatibility of some Linux distributions, it will be necessary to make a separate build `BouncyHsm.Pkcs11Lib`.

## Prerequisites
- _clang_ compiler or _gcc_
- _make_
- _git_

## Example compilation for Rocky Linux 9/AlmaLinux OS 9
Install prerequisites:

```
yum install make gcc clang git -y
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

Edit Makefile:  
Edit `BouncyHsm/build_linux/Makefile` and change `CC=clang` to `CC=gcc`.

Build:
```
cd BouncyHsm
# git checkout <last tag>
cd build_linux
make
```

The compiled library is named `BouncyHsm.Pkcs11Lib-x64.so`.
