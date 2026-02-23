/*
 * Linux: gcc -std=c11 -Wall -Wextra -o list_slots main.c -ldl
 * Windows: gcc -std=c11 -Wall -Wextra -o list_slots.exe main.c
 * Usage: list_slots <path_to_pkcs11_library>
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef _WIN32
  #include <windows.h>
  #define DYNLIB_HANDLE       HMODULE
  #define DYNLIB_LOAD(path)   LoadLibraryA(path)
  #define DYNLIB_GETSYM(h,s)  GetProcAddress(h, s)
  #define DYNLIB_CLOSE(h)     FreeLibrary(h)
#else
  #include <dlfcn.h>
  #define DYNLIB_HANDLE       void*
  #define DYNLIB_LOAD(path)   dlopen(path, RTLD_NOW)
  #define DYNLIB_GETSYM(h,s)  dlsym(h, s)
  #define DYNLIB_CLOSE(h)     dlclose(h)
#endif


#ifdef _WIN32
#include <windows.h>
#include <process.h>

#pragma pack(push, cryptoki, 1)

#define CK_CALL_SPEC __cdecl 

#define CK_PTR *
#define CK_DEFINE_FUNCTION(returnType, name) returnType CK_CALL_SPEC name
#define CK_DECLARE_FUNCTION(returnType, name) returnType CK_CALL_SPEC __declspec(dllimport) name
#define CK_DECLARE_FUNCTION_POINTER(returnType, name) returnType (CK_CALL_SPEC CK_PTR name)
#define CK_CALLBACK_FUNCTION(returnType, name) returnType (CK_CALL_SPEC CK_PTR name)

#ifndef NULL_PTR
#define NULL_PTR 0
#endif

#include "cryptoki/pkcs11.h"

#pragma pack(pop, cryptoki)

#else // #ifdef _WIN32

#define CK_PTR *
#define CK_DEFINE_FUNCTION(returnType, name) returnType name
#define CK_DECLARE_FUNCTION(returnType, name) returnType name
#define CK_DECLARE_FUNCTION_POINTER(returnType, name) returnType (* name)
#define CK_CALLBACK_FUNCTION(returnType, name) returnType (* name)

#ifndef NULL_PTR
#define NULL_PTR 0
#endif

#include "cryptoki/pkcs11.h"

#endif

static void print_ck_rv(CK_RV rv)
{
    printf("CK_RV = 0x%lX\n", (unsigned long)rv);
}

int main(int argc, char *argv[])
{
    if (argc < 2) 
    {
        fprintf(stderr, "Usage: %s <pkcs11_library>\n", argv[0]);
        return 1;
    }

    const char *lib_path = argv[1];

    DYNLIB_HANDLE lib = DYNLIB_LOAD(lib_path);
    if (!lib) 
    {
#ifdef _WIN32
        fprintf(stderr, "Can not load PKCS11 library: %s (GetLastError=%lu)\n", lib_path, GetLastError());
#else
        fprintf(stderr, "Can not load PKCS11 library: %s (%s)\n", lib_path, dlerror());
#endif
        return 1;
    }

    CK_RV (*pC_GetFunctionList)(CK_FUNCTION_LIST_PTR_PTR) = NULL;

    pC_GetFunctionList = (CK_RV (*)(CK_FUNCTION_LIST_PTR_PTR)) DYNLIB_GETSYM(lib, "C_GetFunctionList");

    if (!pC_GetFunctionList) 
    {
        fprintf(stderr, "can not find function C_GetFunctionList\n");
        DYNLIB_CLOSE(lib);
        return EXIT_FAILURE;
    }

    CK_FUNCTION_LIST_PTR pFuncList = NULL;
    CK_RV rv = pC_GetFunctionList(&pFuncList);
    if (rv != CKR_OK || pFuncList == NULL) 
    {
        fprintf(stderr, "C_GetFunctionList failed: ");
        print_ck_rv(rv);
        DYNLIB_CLOSE(lib);
        return EXIT_FAILURE;
    }

    rv = pFuncList->C_Initialize(NULL);
    if (rv != CKR_OK && rv != CKR_CRYPTOKI_ALREADY_INITIALIZED) 
    {
        fprintf(stderr, "C_Initialize failed: ");
        print_ck_rv(rv);
        DYNLIB_CLOSE(lib);
        return EXIT_FAILURE;
    }

    CK_ULONG slotCount = 0;
    rv = pFuncList->C_GetSlotList(CK_FALSE, NULL, &slotCount);
    if (rv != CKR_OK) 
    {
        fprintf(stderr, "C_GetSlotList failed: ");
        print_ck_rv(rv);
        pFuncList->C_Finalize(NULL);
        DYNLIB_CLOSE(lib);
        return EXIT_FAILURE;
    }

    if (slotCount == 0)
    {
        printf("Slots not found.\n");
        pFuncList->C_Finalize(NULL);
        DYNLIB_CLOSE(lib);
        return EXIT_SUCCESS;
    }

    CK_SLOT_ID *slots = (CK_SLOT_ID*)malloc(slotCount *sizeof(CK_SLOT_ID));
    if (slots == NULL) 
    {
        fprintf(stderr, "malloc error.\n");
        pFuncList->C_Finalize(NULL);
        DYNLIB_CLOSE(lib);
        return EXIT_FAILURE;
    }

    rv = pFuncList->C_GetSlotList(CK_FALSE, slots, &slotCount);
    if (rv != CKR_OK) {
        fprintf(stderr, "C_GetSlotList failed: ");
        print_ck_rv(rv);
        free(slots);
        pFuncList->C_Finalize(NULL);
        DYNLIB_CLOSE(lib);
        return EXIT_FAILURE;
    }

    CK_SLOT_INFO slotInfo = { 0 };
    for (CK_ULONG i = 0; i < slotCount; ++i) {

        rv = pFuncList->C_GetSlotInfo(slots[i], &slotInfo);
        if (rv != CKR_OK) {
            fprintf(stderr, "C_GetSlotInfo failed: ");
            print_ck_rv(rv);
            free(slots);
            pFuncList->C_Finalize(NULL);
            DYNLIB_CLOSE(lib);
            return EXIT_FAILURE;
        }

        int len = sizeof(slotInfo.slotDescription);
        while (len > 0 && slotInfo.slotDescription[len - 1] == ' ')
        {
            len--;
        }

        printf("Slot[%lu] ID: %lu\t",
               (unsigned long)i,
               (unsigned long)slots[i]);

        if (slotInfo.flags & CKF_TOKEN_PRESENT == CKF_TOKEN_PRESENT)
        {
            printf("TOKEN_PRESENT: YES ");
        }
        else
        {
            printf("TOKEN_PRESENT:  NO ");
        }

        printf("Description: %.*s\n", len, slotInfo.slotDescription);
    }

    free(slots);
    pFuncList->C_Finalize(NULL);
    DYNLIB_CLOSE(lib);

    return 0;
}
