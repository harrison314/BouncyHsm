SRC_DIR     := ../src/Src/BouncyHsm.Pkcs11Lib
BUILD_DIR   := build
CC          := clang
CFLAGS_BASE := -Wall -Wextra -O2 -fPIC \
               -I$(SRC_DIR) \
               -include $(CURDIR)/shim.h

ARCH_FLAGS_x64   := -arch x86_64
ARCH_FLAGS_arm64 := -arch arm64

SRCS      := $(wildcard $(SRC_DIR)/*.c) \
             $(wildcard $(SRC_DIR)/rpc/*.c) \
             $(wildcard $(SRC_DIR)/utils/*.c)
REL_SRCS  := $(patsubst $(SRC_DIR)/%,%,$(SRCS))
OBJS_x64  := $(patsubst %.c,$(BUILD_DIR)/x64/%.o,$(REL_SRCS))
OBJS_arm64:= $(patsubst %.c,$(BUILD_DIR)/arm64/%.o,$(REL_SRCS))

HDRS := $(wildcard $(SRC_DIR)/*.h) \
        $(wildcard $(SRC_DIR)/rpc/*.h) \
        $(wildcard $(SRC_DIR)/utils/*.h)

.PHONY: all clean universal
all: BouncyHsm.Pkcs11Lib-x64.dylib BouncyHsm.Pkcs11Lib-arm64.dylib

BouncyHsm.Pkcs11Lib-x64.dylib: $(OBJS_x64)
	$(CC) $(CFLAGS_BASE) $(ARCH_FLAGS_x64) -dynamiclib \
	    -install_name @rpath/$@ -o $@ $^

BouncyHsm.Pkcs11Lib-arm64.dylib: $(OBJS_arm64)
	$(CC) $(CFLAGS_BASE) $(ARCH_FLAGS_arm64) -dynamiclib \
	    -install_name @rpath/$@ -o $@ $^

$(BUILD_DIR)/x64/%.o: $(SRC_DIR)/%.c $(HDRS)
	mkdir -p $(@D)
	$(CC) $(CFLAGS_BASE) $(ARCH_FLAGS_x64) -c $< -o $@

$(BUILD_DIR)/arm64/%.o: $(SRC_DIR)/%.c $(HDRS)
	mkdir -p $(@D)
	$(CC) $(CFLAGS_BASE) $(ARCH_FLAGS_arm64) -c $< -o $@

universal: all
	lipo -create \
	    -output BouncyHsm.Pkcs11Lib.dylib \
	    BouncyHsm.Pkcs11Lib-x64.dylib \
	    BouncyHsm.Pkcs11Lib-arm64.dylib

clean:
	rm -rf $(BUILD_DIR)
	rm -f BouncyHsm.Pkcs11Lib*.dylib
