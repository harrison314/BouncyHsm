#ifndef BOUNCY_PKCS11_UTILS_H
#define BOUNCY_PKCS11_UTILS_H

#include "bouncy-pkcs11.h"





void SetPaddedStrSafe(CK_UTF8CHAR* destination, size_t destinationSize, const char* src);

CK_ULONG ConvertCkSpecialUint(CkSpecialUint value);

// Attributes utils

#define AttrValueFromNative_TypeHint_Binary 0x01
#define AttrValueFromNative_TypeHint_Bool 0x02
#define AttrValueFromNative_TypeHint_CkUlong 0x04
#define AttrValueFromNative_TypeHint_CkDate 0x08

#define AttrValueToNative_TypeHint_Binary 0x01
#define AttrValueToNative_TypeHint_Bool 0x02
#define AttrValueToNative_TypeHint_CkUlong 0x04
#define AttrValueToNative_TypeHint_CkDate 0x08

AttrValueFromNative* ConvertToAttrValueFromNative(CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount);

void AttrValueFromNative_Destroy(AttrValueFromNative* ptr, CK_ULONG ulCount);

// Mechanism utils
int MechanismValue_Create(MechanismValue* value, CK_MECHANISM_PTR pMechanism);
void MechanismValue_Destroy(MechanismValue* value);

#endif //BOUNCY_PKCS11_UTILS_H