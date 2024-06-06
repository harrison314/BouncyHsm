# Building PKCS#11 library
Due to the mutual incompatibility of some Linux distributions, it will be necessary to make a separate build `BouncyHsm.Pkcs11Lib`.

## Prerequisites
- _clang_ compiler or _gcc_
- _make_
- _git_

## Example compilation for CentOs7/Rocky Linux 9
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
cd BouncyHsm/build_linux
make
```
