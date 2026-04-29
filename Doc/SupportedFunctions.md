# Supported PKCS#11 funtions

Supported PKCS#11 functions for _Bouncy Hsm_ version 2.1.0.0 (commit _a9c6a4655de7a4740acbe50be50143c107f297eb_).


| Function | Is supported |
| ---  | --- |
| C_Initialize | ✓ |
| C_Finalize | ✓ |
| C_GetInfo | ✓ |
| C_GetFunctionList | ✓<sub>1</sub> |
| C_GetSlotList | ✓ |
| C_GetSlotInfo | ✓ |
| C_GetTokenInfo | ✓ |
| C_GetMechanismList | ✓ |
| C_GetMechanismInfo | ✓ |
| C_InitToken | ✓ |
| C_InitPIN | ✓ |
| C_SetPIN | ✓ |
| C_OpenSession | ✓ |
| C_CloseSession | ✓ |
| C_CloseAllSessions | ✓ |
| C_GetSessionInfo | ✓ |
| C_GetOperationState |  |
| C_SetOperationState |  |
| C_Login | ✓ |
| C_Logout | ✓ |
| C_CreateObject | ✓ |
| C_CopyObject | ✓ |
| C_DestroyObject | ✓ |
| C_GetObjectSize | ✓ |
| C_GetAttributeValue | ✓ |
| C_SetAttributeValue | ✓ |
| C_FindObjectsInit | ✓ |
| C_FindObjects | ✓ |
| C_FindObjectsFinal | ✓ |
| C_EncryptInit | ✓ |
| C_Encrypt | ✓ |
| C_EncryptUpdate | ✓ |
| C_EncryptFinal | ✓ |
| C_DecryptInit | ✓ |
| C_Decrypt | ✓ |
| C_DecryptUpdate | ✓ |
| C_DecryptFinal | ✓ |
| C_DigestInit | ✓ |
| C_Digest | ✓ |
| C_DigestUpdate | ✓ |
| C_DigestKey | ✓ |
| C_DigestFinal | ✓ |
| C_SignInit | ✓ |
| C_Sign | ✓ |
| C_SignUpdate | ✓ |
| C_SignFinal | ✓ |
| C_SignRecoverInit | ✓ |
| C_SignRecover | ✓ |
| C_VerifyInit | ✓ |
| C_Verify | ✓ |
| C_VerifyUpdate | ✓ |
| C_VerifyFinal | ✓ |
| C_VerifyRecoverInit | ✓ |
| C_VerifyRecover | ✓ |
| C_DigestEncryptUpdate |  |
| C_DecryptDigestUpdate |  |
| C_SignEncryptUpdate |  |
| C_DecryptVerifyUpdate |  |
| C_GenerateKey | ✓ |
| C_GenerateKeyPair | ✓ |
| C_WrapKey | ✓ |
| C_UnwrapKey | ✓ |
| C_DeriveKey | ✓ |
| C_SeedRandom | ✓ |
| C_GenerateRandom | ✓ |
| C_GetFunctionStatus | ✓<sub>1</sub> |
| C_CancelFunction | ✓<sub>1</sub> |
| C_WaitForSlotEvent | ✓ |
| C_GetInterfaceList | ✓<sub>1</sub> |
| C_GetInterface | ✓<sub>1</sub> |
| C_LoginUser |  |
| C_SessionCancel | ✓ |
| C_MessageEncryptInit |  |
| C_EncryptMessage |  |
| C_EncryptMessageBegin |  |
| C_EncryptMessageNext |  |
| C_MessageEncryptFinal |  |
| C_MessageDecryptInit |  |
| C_DecryptMessage |  |
| C_DecryptMessageBegin |  |
| C_DecryptMessageNext |  |
| C_MessageDecryptFinal |  |
| C_MessageSignInit |  |
| C_SignMessage |  |
| C_SignMessageBegin |  |
| C_SignMessageNext |  |
| C_MessageSignFinal |  |
| C_MessageVerifyInit |  |
| C_VerifyMessage |  |
| C_VerifyMessageBegin |  |
| C_VerifyMessageNext |  |
| C_MessageVerifyFinal |  |
| C_EncapsulateKey | ✓ |
| C_DecapsulateKey | ✓ |
| C_VerifySignatureInit |  |
| C_VerifySignature |  |
| C_VerifySignatureUpdate |  |
| C_VerifySignatureFinal |  |
| C_GetSessionValidationFlags | ✓ |
| C_AsyncComplete |  |
| C_AsyncGetID |  |
| C_AsyncJoin |  |
| C_WrapKeyAuthenticated |  |
| C_UnwrapKeyAuthenticated |  |

1. The function is implemented only in the native library, so it does not call the server and its call will not appear in the logs.

