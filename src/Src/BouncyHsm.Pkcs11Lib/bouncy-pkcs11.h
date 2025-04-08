#include <stdio.h>
#include <string.h>


#ifdef _WIN32


// PKCS#11 related stuff
#pragma pack(push, cryptoki, 1)

#define CK_IMPORT_SPEC __declspec(dllimport) 

#ifdef CRYPTOKI_EXPORTS 
#define CK_EXPORT_SPEC __declspec(dllexport) 
#else 
#define CK_EXPORT_SPEC CK_IMPORT_SPEC 
#endif 

#define CK_CALL_SPEC __cdecl 

#define CK_PTR *
#define CK_DEFINE_FUNCTION(returnType, name) returnType CK_EXPORT_SPEC CK_CALL_SPEC name
#define CK_DECLARE_FUNCTION(returnType, name) returnType CK_EXPORT_SPEC CK_CALL_SPEC name
#define CK_DECLARE_FUNCTION_POINTER(returnType, name) returnType CK_IMPORT_SPEC (CK_CALL_SPEC CK_PTR name)
#define CK_CALLBACK_FUNCTION(returnType, name) returnType (CK_CALL_SPEC CK_PTR name)

#ifndef NULL_PTR
#define NULL_PTR 0
#endif

#include "cryptoki\pkcs11.h"
#include "cryptoki\pkcs11t_v3_1.h"

#pragma pack(pop, cryptoki)


#else // #ifdef _WIN32


// PKCS#11 related stuff
#define CK_PTR *
#define CK_DEFINE_FUNCTION(returnType, name) returnType name
#define CK_DECLARE_FUNCTION(returnType, name) returnType name
#define CK_DECLARE_FUNCTION_POINTER(returnType, name) returnType (* name)
#define CK_CALLBACK_FUNCTION(returnType, name) returnType (* name)

#ifndef NULL_PTR
#define NULL_PTR 0
#endif

#include <cryptoki/pkcs11.h>
#include "cryptoki/pkcs11t_v3_1.h"

#endif // #ifdef _WIN32
