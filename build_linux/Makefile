SRC_DIR=../src/Src/BouncyHsm.Pkcs11Lib

CC= clang
#CC= gcc
ARCH_FLAGS=
CFLAGS= $(ARCH_FLAGS) -Wall -Wextra -O2 -I$(SRC_DIR)
# -Werror
HEADERFILES=$(SRC_DIR)/*.h $(SRC_DIR)/rpc/*.h $(SRC_DIR)/utils/*.h
LIBNAME=BouncyHsm.Pkcs11Lib-x86.so


all: ARCH_FLAGS += -m64
all: LIBNAME=BouncyHsm.Pkcs11Lib-x64.so
all: build

build64: ARCH_FLAGS += -m64
build64: LIBNAME=BouncyHsm.Pkcs11Lib-x64.so
build64: build clean

build32: ARCH_FLAGS += -m32
build32: LIBNAME=BouncyHsm.Pkcs11Lib-x86.so
build32: build clean

build: globalContext.o logger.o dllMain.o cmp.o rpc.o tcpTransport.o bouncy-pkcs11.o platformHelper.o
	$(CC) $(ARCH_FLAGS) -shared -o $(LIBNAME) \
	-Wl,-soname,$(LIBNAME) \
	-Wl,--version-script,bouncyhsm.pkcs11.version \
	globalContext.o logger.o dllMain.o cmp.o rpc.o tcpTransport.o bouncy-pkcs11.o platformHelper.o \
	-lc -ldl -lresolv
	strip --strip-all $(LIBNAME)

platformHelper.o: $(SRC_DIR)/platformHelper.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/platformHelper.c

globalContext.o: $(SRC_DIR)/globalContext.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/globalContext.c

logger.o: $(SRC_DIR)/logger.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/logger.c

dllMain.o: $(SRC_DIR)/dllMain.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/dllMain.c

cmp.o: $(SRC_DIR)/utils/cmp.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/utils/cmp.c

rpc.o: $(SRC_DIR)/rpc/rpc.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/rpc/rpc.c

tcpTransport.o: $(SRC_DIR)/rpc/tcpTransport.c $(HEADERFILES)
	$(CC) $(CFLAGS) -fPIC -c $(SRC_DIR)/rpc/tcpTransport.c

bouncy-pkcs11.o: $(SRC_DIR)/bouncy-pkcs11.c $(HEADERFILES)
	$(CC) $(CFLAGS) -Wno-unused-parameter -fPIC -c $(SRC_DIR)/bouncy-pkcs11.c

clean:
	-rm -f *.o
	-rm -f *.so

cleanso:
	-rm -f *.so
