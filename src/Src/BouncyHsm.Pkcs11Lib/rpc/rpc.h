// This file is generated.
#ifndef NMRPC_rpc
#define NMRPC_rpc
#include <stddef.h>
#include <stdbool.h>
#include <stdint.h>
// From https://github.com/camgunz/cmp
#include "../utils/cmp.h"

#define NMRPC_OK 0
#define NMRPC_BAD_ARGUMENT 1
#define NMRPC_DESERIALIZE_ERR 2
#define NMRPC_FATAL_ERROR 3

typedef struct _AppIdentification AppIdentification;
typedef struct _ExtendedClientInfo ExtendedClientInfo;
typedef struct _PingRequest PingRequest;
typedef struct _PingEnvelope PingEnvelope;
typedef struct _InitializeRequest InitializeRequest;
typedef struct _InitializeEnvelope InitializeEnvelope;
typedef struct _FinalizeRequest FinalizeRequest;
typedef struct _FinalizeEnvelope FinalizeEnvelope;
typedef struct _GetInfoRequest GetInfoRequest;
typedef struct _GetInfoEnvelope GetInfoEnvelope;
typedef struct _GetSlotListRequest GetSlotListRequest;
typedef struct _GetSlotListEnvelope GetSlotListEnvelope;
typedef struct _GetSlotInfoRequest GetSlotInfoRequest;
typedef struct _CkVersion CkVersion;
typedef struct _CkSpecialUint CkSpecialUint;
typedef struct _SlotInfo SlotInfo;
typedef struct _GetSlotInfoEnvelope GetSlotInfoEnvelope;
typedef struct _GetTokenInfoRequest GetTokenInfoRequest;
typedef struct _TokenInfo TokenInfo;
typedef struct _GetTokenInfoEnvelope GetTokenInfoEnvelope;
typedef struct _GetMechanismListRequest GetMechanismListRequest;
typedef struct _MechanismList MechanismList;
typedef struct _GetMechanismListEnvelope GetMechanismListEnvelope;
typedef struct _GetMechanismInfoRequest GetMechanismInfoRequest;
typedef struct _MechanismInfo MechanismInfo;
typedef struct _GetMechanismInfoEnvelope GetMechanismInfoEnvelope;
typedef struct _SetPinRequest SetPinRequest;
typedef struct _SetPinEnvelope SetPinEnvelope;
typedef struct _OpenSessionRequest OpenSessionRequest;
typedef struct _OpenSessionEnvelope OpenSessionEnvelope;
typedef struct _CloseSessionRequest CloseSessionRequest;
typedef struct _CloseSessionEnvelope CloseSessionEnvelope;
typedef struct _CloseAllSessionsRequest CloseAllSessionsRequest;
typedef struct _CloseAllSessionsEnvelope CloseAllSessionsEnvelope;
typedef struct _GetSessionInfoRequest GetSessionInfoRequest;
typedef struct _SessionInfoData SessionInfoData;
typedef struct _GetSessionInfoEnvelope GetSessionInfoEnvelope;
typedef struct _LoginRequest LoginRequest;
typedef struct _LoginEnvelope LoginEnvelope;
typedef struct _LogoutRequest LogoutRequest;
typedef struct _LogoutEnvelope LogoutEnvelope;
typedef struct _SeedRandomRequest SeedRandomRequest;
typedef struct _SeedRandomEnvelope SeedRandomEnvelope;
typedef struct _GenerateRandomRequest GenerateRandomRequest;
typedef struct _GenerateRandomEnvelope GenerateRandomEnvelope;
typedef struct _MechanismValue MechanismValue;
typedef struct _DigestInitRequest DigestInitRequest;
typedef struct _DigestInitEnvelope DigestInitEnvelope;
typedef struct _DigestRequest DigestRequest;
typedef struct _DigestValue DigestValue;
typedef struct _DigestEnvelope DigestEnvelope;
typedef struct _DigestUpdateRequest DigestUpdateRequest;
typedef struct _DigestUpdateEnvelope DigestUpdateEnvelope;
typedef struct _DigestKeyRequest DigestKeyRequest;
typedef struct _DigestKeyEnvelope DigestKeyEnvelope;
typedef struct _DigestFinalRequest DigestFinalRequest;
typedef struct _DigestFinalEnvelope DigestFinalEnvelope;
typedef struct _AttrValueFromNative AttrValueFromNative;
typedef struct _CreateObjectRequest CreateObjectRequest;
typedef struct _CreateObjectEnvelope CreateObjectEnvelope;
typedef struct _DestroyObjectRequest DestroyObjectRequest;
typedef struct _DestroyObjectEnvelope DestroyObjectEnvelope;
typedef struct _FindObjectsInitRequest FindObjectsInitRequest;
typedef struct _FindObjectsInitEnvelope FindObjectsInitEnvelope;
typedef struct _FindObjectsRequest FindObjectsRequest;
typedef struct _FindObjectsData FindObjectsData;
typedef struct _FindObjectsEnvelope FindObjectsEnvelope;
typedef struct _FindObjectsFinalRequest FindObjectsFinalRequest;
typedef struct _FindObjectsFinalEnvelope FindObjectsFinalEnvelope;
typedef struct _GetObjectSizeRequest GetObjectSizeRequest;
typedef struct _GetObjectSizeEnvelope GetObjectSizeEnvelope;
typedef struct _GetAttributeInputValues GetAttributeInputValues;
typedef struct _GetAttributeValueRequest GetAttributeValueRequest;
typedef struct _GetAttributeOutValue GetAttributeOutValue;
typedef struct _GetAttributeOutValues GetAttributeOutValues;
typedef struct _GetAttributeValueEnvelope GetAttributeValueEnvelope;
typedef struct _GenerateKeyPairRequest GenerateKeyPairRequest;
typedef struct _GenerateKeyPairData GenerateKeyPairData;
typedef struct _GenerateKeyPairEnvelope GenerateKeyPairEnvelope;
typedef struct _SignInitRequest SignInitRequest;
typedef struct _SignInitEnvelope SignInitEnvelope;
typedef struct _SignRequest SignRequest;
typedef struct _SignatureData SignatureData;
typedef struct _SignEnvelope SignEnvelope;
typedef struct _SignUpdateRequest SignUpdateRequest;
typedef struct _SignUpdateEnvelope SignUpdateEnvelope;
typedef struct _SignFinalRequest SignFinalRequest;
typedef struct _SignFinalEnvelope SignFinalEnvelope;
typedef struct _VerifyInitRequest VerifyInitRequest;
typedef struct _VerifyInitEnvelope VerifyInitEnvelope;
typedef struct _VerifyRequest VerifyRequest;
typedef struct _VerifyEnvelope VerifyEnvelope;
typedef struct _VerifyUpdateRequest VerifyUpdateRequest;
typedef struct _VerifyUpdateEnvelope VerifyUpdateEnvelope;
typedef struct _VerifyFinalRequest VerifyFinalRequest;
typedef struct _VerifyFinalEnvelope VerifyFinalEnvelope;
typedef struct _GenerateKeyRequest GenerateKeyRequest;
typedef struct _GenerateKeyData GenerateKeyData;
typedef struct _GenerateKeyEnvelope GenerateKeyEnvelope;
typedef struct _DeriveKeyRequest DeriveKeyRequest;
typedef struct _DeriveKeyData DeriveKeyData;
typedef struct _DeriveKeyEnvelope DeriveKeyEnvelope;
typedef struct _EncryptInitRequest EncryptInitRequest;
typedef struct _EncryptInitEnvelope EncryptInitEnvelope;
typedef struct _EncryptRequest EncryptRequest;
typedef struct _EncryptData EncryptData;
typedef struct _EncryptEnvelope EncryptEnvelope;
typedef struct _EncryptUpdateRequest EncryptUpdateRequest;
typedef struct _EncryptUpdateEnvelope EncryptUpdateEnvelope;
typedef struct _EncryptFinalRequest EncryptFinalRequest;
typedef struct _EncryptFinalEnvelope EncryptFinalEnvelope;
typedef struct _DecryptInitRequest DecryptInitRequest;
typedef struct _DecryptInitEnvelope DecryptInitEnvelope;
typedef struct _DecryptRequest DecryptRequest;
typedef struct _DecryptData DecryptData;
typedef struct _DecryptEnvelope DecryptEnvelope;
typedef struct _DecryptUpdateRequest DecryptUpdateRequest;
typedef struct _DecryptUpdateEnvelope DecryptUpdateEnvelope;
typedef struct _DecryptFinalRequest DecryptFinalRequest;
typedef struct _DecryptFinalEnvelope DecryptFinalEnvelope;
typedef struct _WrapKeyRequest WrapKeyRequest;
typedef struct _WrapKeyData WrapKeyData;
typedef struct _WrapKeyEnvelope WrapKeyEnvelope;
typedef struct _UnwrapKeyRequest UnwrapKeyRequest;
typedef struct _UnwrapKeyData UnwrapKeyData;
typedef struct _UnwrapKeyEnvelope UnwrapKeyEnvelope;
typedef struct _SetAttributeValueRequest SetAttributeValueRequest;
typedef struct _SetAttributeValueEnvelope SetAttributeValueEnvelope;
typedef struct _CopyObjectRequest CopyObjectRequest;
typedef struct _CopyObjectData CopyObjectData;
typedef struct _CopyObjectEnvelope CopyObjectEnvelope;
typedef struct _WaitForSlotEventRequest WaitForSlotEventRequest;
typedef struct _WaitForSlotEventEnvelope WaitForSlotEventEnvelope;
typedef struct _WaitForSlotEventData WaitForSlotEventData;
typedef struct _SignRecoverInitRequest SignRecoverInitRequest;
typedef struct _SignRecoverInitEnvelope SignRecoverInitEnvelope;
typedef struct _SignRecoverRequest SignRecoverRequest;
typedef struct _SignRecoverData SignRecoverData;
typedef struct _SignRecoverEnvelope SignRecoverEnvelope;
typedef struct _VerifyRecoverInitRequest VerifyRecoverInitRequest;
typedef struct _VerifyRecoverInitEnvelope VerifyRecoverInitEnvelope;
typedef struct _VerifyRecoverRequest VerifyRecoverRequest;
typedef struct _VerifyRecoverData VerifyRecoverData;
typedef struct _VerifyRecoverEnvelope VerifyRecoverEnvelope;
typedef struct _CkP_MacGeneralParams CkP_MacGeneralParams;
typedef struct _CkP_ExtractParams CkP_ExtractParams;
typedef struct _CkP_RsaPkcsPssParams CkP_RsaPkcsPssParams;
typedef struct _CkP_RawDataParams CkP_RawDataParams;
typedef struct _CkP_KeyDerivationStringData CkP_KeyDerivationStringData;
typedef struct _CkP_CkObjectHandle CkP_CkObjectHandle;
typedef struct _Ckp_CkEcdh1DeriveParams Ckp_CkEcdh1DeriveParams;
typedef struct _Ckp_CkGcmParams Ckp_CkGcmParams;
typedef struct _Ckp_CkCcmParams Ckp_CkCcmParams;
typedef struct _Ckp_CkRsaPkcsOaepParams Ckp_CkRsaPkcsOaepParams;
typedef struct _Ckp_CkAesCbcEnryptDataParams Ckp_CkAesCbcEnryptDataParams;
typedef struct _Ckp_CkChaCha20Params Ckp_CkChaCha20Params;
typedef struct _Ckp_CkSalsa20Params Ckp_CkSalsa20Params;
typedef struct _Ckp_CkSalsa20ChaCha20Poly1305Params Ckp_CkSalsa20ChaCha20Poly1305Params;
typedef struct _Ckp_CkEddsaParams Ckp_CkEddsaParams;

typedef struct _Binary Binary;

typedef struct _ArrayOfuint32_t ArrayOfuint32_t;
typedef struct _ArrayOfAttrValueFromNative ArrayOfAttrValueFromNative;
typedef struct _ArrayOfGetAttributeInputValues ArrayOfGetAttributeInputValues;
typedef struct _ArrayOfGetAttributeOutValue ArrayOfGetAttributeOutValue;

typedef struct _Binary {
  uint8_t* data;
  size_t size;
} Binary;

int Binary_Release(Binary* value);

typedef int (*SerializeFnPtr_t)(cmp_ctx_t* ctx, void* data);
int nmrpc_writeAsBinary(void *data, SerializeFnPtr_t serialize, Binary** outBinary);

typedef struct _ArrayOfuint32_t 
{
    uint32_t* array;
    int length;
} ArrayOfuint32_t;
int ArrayOfuint32_t_Serialize(cmp_ctx_t* ctx, ArrayOfuint32_t* value);
int ArrayOfuint32_t_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfuint32_t* value);
int ArrayOfuint32_t_Release(ArrayOfuint32_t* value);

typedef struct _ArrayOfAttrValueFromNative 
{
    AttrValueFromNative* array;
    int length;
} ArrayOfAttrValueFromNative;
int ArrayOfAttrValueFromNative_Serialize(cmp_ctx_t* ctx, ArrayOfAttrValueFromNative* value);
int ArrayOfAttrValueFromNative_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfAttrValueFromNative* value);
int ArrayOfAttrValueFromNative_Release(ArrayOfAttrValueFromNative* value);

typedef struct _ArrayOfGetAttributeInputValues 
{
    GetAttributeInputValues* array;
    int length;
} ArrayOfGetAttributeInputValues;
int ArrayOfGetAttributeInputValues_Serialize(cmp_ctx_t* ctx, ArrayOfGetAttributeInputValues* value);
int ArrayOfGetAttributeInputValues_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfGetAttributeInputValues* value);
int ArrayOfGetAttributeInputValues_Release(ArrayOfGetAttributeInputValues* value);

typedef struct _ArrayOfGetAttributeOutValue 
{
    GetAttributeOutValue* array;
    int length;
} ArrayOfGetAttributeOutValue;
int ArrayOfGetAttributeOutValue_Serialize(cmp_ctx_t* ctx, ArrayOfGetAttributeOutValue* value);
int ArrayOfGetAttributeOutValue_Deserialize(cmp_ctx_t* ctx, cmp_object_t* start_obj_ptr, ArrayOfGetAttributeOutValue* value);
int ArrayOfGetAttributeOutValue_Release(ArrayOfGetAttributeOutValue* value);


typedef struct _AppIdentification
{
    char* AppName;
    char* AppNonce;
    uint64_t Pid;
} AppIdentification;

int AppIdentification_Serialize(cmp_ctx_t* ctx, AppIdentification* value);
int AppIdentification_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, AppIdentification* value);
int AppIdentification_Release(AppIdentification* value);

typedef struct _ExtendedClientInfo
{
    uint32_t CkUlongSize;
    uint32_t PointerSize;
    char* CompiuterName;
    char* Platform;
    char* LibVersion;
} ExtendedClientInfo;

int ExtendedClientInfo_Serialize(cmp_ctx_t* ctx, ExtendedClientInfo* value);
int ExtendedClientInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, ExtendedClientInfo* value);
int ExtendedClientInfo_Release(ExtendedClientInfo* value);

typedef struct _PingRequest
{
    AppIdentification AppId;
} PingRequest;

int PingRequest_Serialize(cmp_ctx_t* ctx, PingRequest* value);
int PingRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, PingRequest* value);
int PingRequest_Release(PingRequest* value);

typedef struct _PingEnvelope
{
    uint32_t Rv;
} PingEnvelope;

int PingEnvelope_Serialize(cmp_ctx_t* ctx, PingEnvelope* value);
int PingEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, PingEnvelope* value);
int PingEnvelope_Release(PingEnvelope* value);

typedef struct _InitializeRequest
{
    AppIdentification AppId;
    bool IsMutexFnSet;
    bool LibraryCantCreateOsThreads;
    bool OsLockingOk;
    ExtendedClientInfo ClientInfo;
} InitializeRequest;

int InitializeRequest_Serialize(cmp_ctx_t* ctx, InitializeRequest* value);
int InitializeRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, InitializeRequest* value);
int InitializeRequest_Release(InitializeRequest* value);

typedef struct _InitializeEnvelope
{
    uint32_t Rv;
} InitializeEnvelope;

int InitializeEnvelope_Serialize(cmp_ctx_t* ctx, InitializeEnvelope* value);
int InitializeEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, InitializeEnvelope* value);
int InitializeEnvelope_Release(InitializeEnvelope* value);

typedef struct _FinalizeRequest
{
    AppIdentification AppId;
    bool IsPtrSet;
} FinalizeRequest;

int FinalizeRequest_Serialize(cmp_ctx_t* ctx, FinalizeRequest* value);
int FinalizeRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FinalizeRequest* value);
int FinalizeRequest_Release(FinalizeRequest* value);

typedef struct _FinalizeEnvelope
{
    uint32_t Rv;
} FinalizeEnvelope;

int FinalizeEnvelope_Serialize(cmp_ctx_t* ctx, FinalizeEnvelope* value);
int FinalizeEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FinalizeEnvelope* value);
int FinalizeEnvelope_Release(FinalizeEnvelope* value);

typedef struct _GetInfoRequest
{
    AppIdentification AppId;
} GetInfoRequest;

int GetInfoRequest_Serialize(cmp_ctx_t* ctx, GetInfoRequest* value);
int GetInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetInfoRequest* value);
int GetInfoRequest_Release(GetInfoRequest* value);

typedef struct _GetInfoEnvelope
{
    uint32_t Rv;
    char* ManufacturerID;
} GetInfoEnvelope;

int GetInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetInfoEnvelope* value);
int GetInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetInfoEnvelope* value);
int GetInfoEnvelope_Release(GetInfoEnvelope* value);

typedef struct _GetSlotListRequest
{
    AppIdentification AppId;
    bool IsTokenPresent;
    bool IsSlotListPointerPresent;
    uint32_t PullCount;
} GetSlotListRequest;

int GetSlotListRequest_Serialize(cmp_ctx_t* ctx, GetSlotListRequest* value);
int GetSlotListRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetSlotListRequest* value);
int GetSlotListRequest_Release(GetSlotListRequest* value);

typedef struct _GetSlotListEnvelope
{
    uint32_t Rv;
    uint32_t PullCount;
    ArrayOfuint32_t Slots;
} GetSlotListEnvelope;

int GetSlotListEnvelope_Serialize(cmp_ctx_t* ctx, GetSlotListEnvelope* value);
int GetSlotListEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetSlotListEnvelope* value);
int GetSlotListEnvelope_Release(GetSlotListEnvelope* value);

typedef struct _GetSlotInfoRequest
{
    AppIdentification AppId;
    uint32_t SlotId;
} GetSlotInfoRequest;

int GetSlotInfoRequest_Serialize(cmp_ctx_t* ctx, GetSlotInfoRequest* value);
int GetSlotInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetSlotInfoRequest* value);
int GetSlotInfoRequest_Release(GetSlotInfoRequest* value);

typedef struct _CkVersion
{
    int32_t Major;
    int32_t Minor;
} CkVersion;

int CkVersion_Serialize(cmp_ctx_t* ctx, CkVersion* value);
int CkVersion_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkVersion* value);
int CkVersion_Release(CkVersion* value);

typedef struct _CkSpecialUint
{
    bool UnavailableInformation;
    bool EffectivelyInfinite;
    bool InformationSensitive;
    uint32_t Value;
} CkSpecialUint;

int CkSpecialUint_Serialize(cmp_ctx_t* ctx, CkSpecialUint* value);
int CkSpecialUint_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkSpecialUint* value);
int CkSpecialUint_Release(CkSpecialUint* value);

typedef struct _SlotInfo
{
    char* SlotDescription;
    char* ManufacturerID;
    bool FlagsTokenPresent;
    bool FlagsRemovableDevice;
    bool FlagsHwSlot;
    CkVersion HardwareVersion;
    CkVersion FirmwareVersion;
} SlotInfo;

int SlotInfo_Serialize(cmp_ctx_t* ctx, SlotInfo* value);
int SlotInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SlotInfo* value);
int SlotInfo_Release(SlotInfo* value);

typedef struct _GetSlotInfoEnvelope
{
    uint32_t Rv;
    SlotInfo* Data;
} GetSlotInfoEnvelope;

int GetSlotInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetSlotInfoEnvelope* value);
int GetSlotInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetSlotInfoEnvelope* value);
int GetSlotInfoEnvelope_Release(GetSlotInfoEnvelope* value);

typedef struct _GetTokenInfoRequest
{
    AppIdentification AppId;
    uint32_t SlotId;
} GetTokenInfoRequest;

int GetTokenInfoRequest_Serialize(cmp_ctx_t* ctx, GetTokenInfoRequest* value);
int GetTokenInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetTokenInfoRequest* value);
int GetTokenInfoRequest_Release(GetTokenInfoRequest* value);

typedef struct _TokenInfo
{
    char* Label;
    char* ManufacturerId;
    char* Model;
    char* SerialNumber;
    uint32_t Flags;
    CkSpecialUint MaxSessionCount;
    CkSpecialUint SessionCount;
    CkSpecialUint MaxRwSessionCount;
    CkSpecialUint RwSessionCount;
    uint32_t MaxPinLen;
    uint32_t MinPinLen;
    CkSpecialUint TotalPublicMemory;
    CkSpecialUint FreePublicMemory;
    CkSpecialUint TotalPrivateMemory;
    CkSpecialUint FreePrivateMemory;
    CkVersion HardwareVersion;
    CkVersion FirmwareVersion;
    char* UtcTime;
} TokenInfo;

int TokenInfo_Serialize(cmp_ctx_t* ctx, TokenInfo* value);
int TokenInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, TokenInfo* value);
int TokenInfo_Release(TokenInfo* value);

typedef struct _GetTokenInfoEnvelope
{
    uint32_t Rv;
    TokenInfo* Data;
} GetTokenInfoEnvelope;

int GetTokenInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetTokenInfoEnvelope* value);
int GetTokenInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetTokenInfoEnvelope* value);
int GetTokenInfoEnvelope_Release(GetTokenInfoEnvelope* value);

typedef struct _GetMechanismListRequest
{
    AppIdentification AppId;
    bool IsMechanismListPointerPresent;
    uint32_t SlotId;
    uint32_t PullCount;
} GetMechanismListRequest;

int GetMechanismListRequest_Serialize(cmp_ctx_t* ctx, GetMechanismListRequest* value);
int GetMechanismListRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetMechanismListRequest* value);
int GetMechanismListRequest_Release(GetMechanismListRequest* value);

typedef struct _MechanismList
{
    uint32_t PullCount;
    ArrayOfuint32_t MechanismTypes;
} MechanismList;

int MechanismList_Serialize(cmp_ctx_t* ctx, MechanismList* value);
int MechanismList_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, MechanismList* value);
int MechanismList_Release(MechanismList* value);

typedef struct _GetMechanismListEnvelope
{
    uint32_t Rv;
    MechanismList* Data;
} GetMechanismListEnvelope;

int GetMechanismListEnvelope_Serialize(cmp_ctx_t* ctx, GetMechanismListEnvelope* value);
int GetMechanismListEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetMechanismListEnvelope* value);
int GetMechanismListEnvelope_Release(GetMechanismListEnvelope* value);

typedef struct _GetMechanismInfoRequest
{
    AppIdentification AppId;
    uint32_t SlotId;
    uint32_t MechanismType;
} GetMechanismInfoRequest;

int GetMechanismInfoRequest_Serialize(cmp_ctx_t* ctx, GetMechanismInfoRequest* value);
int GetMechanismInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetMechanismInfoRequest* value);
int GetMechanismInfoRequest_Release(GetMechanismInfoRequest* value);

typedef struct _MechanismInfo
{
    uint32_t MechanismType;
    uint32_t MinKeySize;
    uint32_t MaxKeySize;
    uint32_t Flags;
} MechanismInfo;

int MechanismInfo_Serialize(cmp_ctx_t* ctx, MechanismInfo* value);
int MechanismInfo_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, MechanismInfo* value);
int MechanismInfo_Release(MechanismInfo* value);

typedef struct _GetMechanismInfoEnvelope
{
    uint32_t Rv;
    MechanismInfo* Data;
} GetMechanismInfoEnvelope;

int GetMechanismInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetMechanismInfoEnvelope* value);
int GetMechanismInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetMechanismInfoEnvelope* value);
int GetMechanismInfoEnvelope_Release(GetMechanismInfoEnvelope* value);

typedef struct _SetPinRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary* Utf8OldPin;
    Binary* Utf8NewPin;
} SetPinRequest;

int SetPinRequest_Serialize(cmp_ctx_t* ctx, SetPinRequest* value);
int SetPinRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SetPinRequest* value);
int SetPinRequest_Release(SetPinRequest* value);

typedef struct _SetPinEnvelope
{
    uint32_t Rv;
} SetPinEnvelope;

int SetPinEnvelope_Serialize(cmp_ctx_t* ctx, SetPinEnvelope* value);
int SetPinEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SetPinEnvelope* value);
int SetPinEnvelope_Release(SetPinEnvelope* value);

typedef struct _OpenSessionRequest
{
    AppIdentification AppId;
    uint32_t SlotId;
    uint32_t Flags;
    bool IsPtrApplicationSet;
    bool IsNotifySet;
} OpenSessionRequest;

int OpenSessionRequest_Serialize(cmp_ctx_t* ctx, OpenSessionRequest* value);
int OpenSessionRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, OpenSessionRequest* value);
int OpenSessionRequest_Release(OpenSessionRequest* value);

typedef struct _OpenSessionEnvelope
{
    uint32_t Rv;
    uint32_t SessionId;
} OpenSessionEnvelope;

int OpenSessionEnvelope_Serialize(cmp_ctx_t* ctx, OpenSessionEnvelope* value);
int OpenSessionEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, OpenSessionEnvelope* value);
int OpenSessionEnvelope_Release(OpenSessionEnvelope* value);

typedef struct _CloseSessionRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
} CloseSessionRequest;

int CloseSessionRequest_Serialize(cmp_ctx_t* ctx, CloseSessionRequest* value);
int CloseSessionRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CloseSessionRequest* value);
int CloseSessionRequest_Release(CloseSessionRequest* value);

typedef struct _CloseSessionEnvelope
{
    uint32_t Rv;
} CloseSessionEnvelope;

int CloseSessionEnvelope_Serialize(cmp_ctx_t* ctx, CloseSessionEnvelope* value);
int CloseSessionEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CloseSessionEnvelope* value);
int CloseSessionEnvelope_Release(CloseSessionEnvelope* value);

typedef struct _CloseAllSessionsRequest
{
    AppIdentification AppId;
    uint32_t SlotId;
} CloseAllSessionsRequest;

int CloseAllSessionsRequest_Serialize(cmp_ctx_t* ctx, CloseAllSessionsRequest* value);
int CloseAllSessionsRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CloseAllSessionsRequest* value);
int CloseAllSessionsRequest_Release(CloseAllSessionsRequest* value);

typedef struct _CloseAllSessionsEnvelope
{
    uint32_t Rv;
} CloseAllSessionsEnvelope;

int CloseAllSessionsEnvelope_Serialize(cmp_ctx_t* ctx, CloseAllSessionsEnvelope* value);
int CloseAllSessionsEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CloseAllSessionsEnvelope* value);
int CloseAllSessionsEnvelope_Release(CloseAllSessionsEnvelope* value);

typedef struct _GetSessionInfoRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
} GetSessionInfoRequest;

int GetSessionInfoRequest_Serialize(cmp_ctx_t* ctx, GetSessionInfoRequest* value);
int GetSessionInfoRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetSessionInfoRequest* value);
int GetSessionInfoRequest_Release(GetSessionInfoRequest* value);

typedef struct _SessionInfoData
{
    uint32_t SlotId;
    uint32_t State;
    uint32_t Flags;
    uint32_t DeviceError;
} SessionInfoData;

int SessionInfoData_Serialize(cmp_ctx_t* ctx, SessionInfoData* value);
int SessionInfoData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SessionInfoData* value);
int SessionInfoData_Release(SessionInfoData* value);

typedef struct _GetSessionInfoEnvelope
{
    uint32_t Rv;
    SessionInfoData* Data;
} GetSessionInfoEnvelope;

int GetSessionInfoEnvelope_Serialize(cmp_ctx_t* ctx, GetSessionInfoEnvelope* value);
int GetSessionInfoEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetSessionInfoEnvelope* value);
int GetSessionInfoEnvelope_Release(GetSessionInfoEnvelope* value);

typedef struct _LoginRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t UserType;
    Binary* Utf8Pin;
} LoginRequest;

int LoginRequest_Serialize(cmp_ctx_t* ctx, LoginRequest* value);
int LoginRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, LoginRequest* value);
int LoginRequest_Release(LoginRequest* value);

typedef struct _LoginEnvelope
{
    uint32_t Rv;
} LoginEnvelope;

int LoginEnvelope_Serialize(cmp_ctx_t* ctx, LoginEnvelope* value);
int LoginEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, LoginEnvelope* value);
int LoginEnvelope_Release(LoginEnvelope* value);

typedef struct _LogoutRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
} LogoutRequest;

int LogoutRequest_Serialize(cmp_ctx_t* ctx, LogoutRequest* value);
int LogoutRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, LogoutRequest* value);
int LogoutRequest_Release(LogoutRequest* value);

typedef struct _LogoutEnvelope
{
    uint32_t Rv;
} LogoutEnvelope;

int LogoutEnvelope_Serialize(cmp_ctx_t* ctx, LogoutEnvelope* value);
int LogoutEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, LogoutEnvelope* value);
int LogoutEnvelope_Release(LogoutEnvelope* value);

typedef struct _SeedRandomRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Seed;
} SeedRandomRequest;

int SeedRandomRequest_Serialize(cmp_ctx_t* ctx, SeedRandomRequest* value);
int SeedRandomRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SeedRandomRequest* value);
int SeedRandomRequest_Release(SeedRandomRequest* value);

typedef struct _SeedRandomEnvelope
{
    uint32_t Rv;
} SeedRandomEnvelope;

int SeedRandomEnvelope_Serialize(cmp_ctx_t* ctx, SeedRandomEnvelope* value);
int SeedRandomEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SeedRandomEnvelope* value);
int SeedRandomEnvelope_Release(SeedRandomEnvelope* value);

typedef struct _GenerateRandomRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t RandomLen;
} GenerateRandomRequest;

int GenerateRandomRequest_Serialize(cmp_ctx_t* ctx, GenerateRandomRequest* value);
int GenerateRandomRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateRandomRequest* value);
int GenerateRandomRequest_Release(GenerateRandomRequest* value);

typedef struct _GenerateRandomEnvelope
{
    uint32_t Rv;
    Binary* Data;
} GenerateRandomEnvelope;

int GenerateRandomEnvelope_Serialize(cmp_ctx_t* ctx, GenerateRandomEnvelope* value);
int GenerateRandomEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateRandomEnvelope* value);
int GenerateRandomEnvelope_Release(GenerateRandomEnvelope* value);

typedef struct _MechanismValue
{
    uint32_t MechanismType;
    Binary* MechanismParamMp;
} MechanismValue;

int MechanismValue_Serialize(cmp_ctx_t* ctx, MechanismValue* value);
int MechanismValue_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, MechanismValue* value);
int MechanismValue_Release(MechanismValue* value);

typedef struct _DigestInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
} DigestInitRequest;

int DigestInitRequest_Serialize(cmp_ctx_t* ctx, DigestInitRequest* value);
int DigestInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestInitRequest* value);
int DigestInitRequest_Release(DigestInitRequest* value);

typedef struct _DigestInitEnvelope
{
    uint32_t Rv;
} DigestInitEnvelope;

int DigestInitEnvelope_Serialize(cmp_ctx_t* ctx, DigestInitEnvelope* value);
int DigestInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestInitEnvelope* value);
int DigestInitEnvelope_Release(DigestInitEnvelope* value);

typedef struct _DigestRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
    bool IsDigestPtrSet;
    uint32_t PulDigestLen;
} DigestRequest;

int DigestRequest_Serialize(cmp_ctx_t* ctx, DigestRequest* value);
int DigestRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestRequest* value);
int DigestRequest_Release(DigestRequest* value);

typedef struct _DigestValue
{
    uint32_t PulDigestLen;
    Binary* Data;
} DigestValue;

int DigestValue_Serialize(cmp_ctx_t* ctx, DigestValue* value);
int DigestValue_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestValue* value);
int DigestValue_Release(DigestValue* value);

typedef struct _DigestEnvelope
{
    uint32_t Rv;
    DigestValue* Data;
} DigestEnvelope;

int DigestEnvelope_Serialize(cmp_ctx_t* ctx, DigestEnvelope* value);
int DigestEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestEnvelope* value);
int DigestEnvelope_Release(DigestEnvelope* value);

typedef struct _DigestUpdateRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
} DigestUpdateRequest;

int DigestUpdateRequest_Serialize(cmp_ctx_t* ctx, DigestUpdateRequest* value);
int DigestUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestUpdateRequest* value);
int DigestUpdateRequest_Release(DigestUpdateRequest* value);

typedef struct _DigestUpdateEnvelope
{
    uint32_t Rv;
} DigestUpdateEnvelope;

int DigestUpdateEnvelope_Serialize(cmp_ctx_t* ctx, DigestUpdateEnvelope* value);
int DigestUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestUpdateEnvelope* value);
int DigestUpdateEnvelope_Release(DigestUpdateEnvelope* value);

typedef struct _DigestKeyRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t ObjectHandle;
} DigestKeyRequest;

int DigestKeyRequest_Serialize(cmp_ctx_t* ctx, DigestKeyRequest* value);
int DigestKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestKeyRequest* value);
int DigestKeyRequest_Release(DigestKeyRequest* value);

typedef struct _DigestKeyEnvelope
{
    uint32_t Rv;
} DigestKeyEnvelope;

int DigestKeyEnvelope_Serialize(cmp_ctx_t* ctx, DigestKeyEnvelope* value);
int DigestKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestKeyEnvelope* value);
int DigestKeyEnvelope_Release(DigestKeyEnvelope* value);

typedef struct _DigestFinalRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    bool IsDigestPtrSet;
    uint32_t PulDigestLen;
} DigestFinalRequest;

int DigestFinalRequest_Serialize(cmp_ctx_t* ctx, DigestFinalRequest* value);
int DigestFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestFinalRequest* value);
int DigestFinalRequest_Release(DigestFinalRequest* value);

typedef struct _DigestFinalEnvelope
{
    uint32_t Rv;
    DigestValue* Data;
} DigestFinalEnvelope;

int DigestFinalEnvelope_Serialize(cmp_ctx_t* ctx, DigestFinalEnvelope* value);
int DigestFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DigestFinalEnvelope* value);
int DigestFinalEnvelope_Release(DigestFinalEnvelope* value);

typedef struct _AttrValueFromNative
{
    uint32_t AttributeType;
    int32_t ValueTypeHint;
    Binary ValueRawBytes;
    bool ValueBool;
    uint32_t ValueCkUlong;
    char* ValueCkDate;
} AttrValueFromNative;

int AttrValueFromNative_Serialize(cmp_ctx_t* ctx, AttrValueFromNative* value);
int AttrValueFromNative_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, AttrValueFromNative* value);
int AttrValueFromNative_Release(AttrValueFromNative* value);

typedef struct _CreateObjectRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    ArrayOfAttrValueFromNative Template;
} CreateObjectRequest;

int CreateObjectRequest_Serialize(cmp_ctx_t* ctx, CreateObjectRequest* value);
int CreateObjectRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CreateObjectRequest* value);
int CreateObjectRequest_Release(CreateObjectRequest* value);

typedef struct _CreateObjectEnvelope
{
    uint32_t Rv;
    uint32_t ObjectHandle;
} CreateObjectEnvelope;

int CreateObjectEnvelope_Serialize(cmp_ctx_t* ctx, CreateObjectEnvelope* value);
int CreateObjectEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CreateObjectEnvelope* value);
int CreateObjectEnvelope_Release(CreateObjectEnvelope* value);

typedef struct _DestroyObjectRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t ObjectHandle;
} DestroyObjectRequest;

int DestroyObjectRequest_Serialize(cmp_ctx_t* ctx, DestroyObjectRequest* value);
int DestroyObjectRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DestroyObjectRequest* value);
int DestroyObjectRequest_Release(DestroyObjectRequest* value);

typedef struct _DestroyObjectEnvelope
{
    uint32_t Rv;
} DestroyObjectEnvelope;

int DestroyObjectEnvelope_Serialize(cmp_ctx_t* ctx, DestroyObjectEnvelope* value);
int DestroyObjectEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DestroyObjectEnvelope* value);
int DestroyObjectEnvelope_Release(DestroyObjectEnvelope* value);

typedef struct _FindObjectsInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    ArrayOfAttrValueFromNative Template;
} FindObjectsInitRequest;

int FindObjectsInitRequest_Serialize(cmp_ctx_t* ctx, FindObjectsInitRequest* value);
int FindObjectsInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsInitRequest* value);
int FindObjectsInitRequest_Release(FindObjectsInitRequest* value);

typedef struct _FindObjectsInitEnvelope
{
    uint32_t Rv;
} FindObjectsInitEnvelope;

int FindObjectsInitEnvelope_Serialize(cmp_ctx_t* ctx, FindObjectsInitEnvelope* value);
int FindObjectsInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsInitEnvelope* value);
int FindObjectsInitEnvelope_Release(FindObjectsInitEnvelope* value);

typedef struct _FindObjectsRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t MaxObjectCount;
} FindObjectsRequest;

int FindObjectsRequest_Serialize(cmp_ctx_t* ctx, FindObjectsRequest* value);
int FindObjectsRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsRequest* value);
int FindObjectsRequest_Release(FindObjectsRequest* value);

typedef struct _FindObjectsData
{
    uint32_t PullObjectCount;
    ArrayOfuint32_t Objects;
} FindObjectsData;

int FindObjectsData_Serialize(cmp_ctx_t* ctx, FindObjectsData* value);
int FindObjectsData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsData* value);
int FindObjectsData_Release(FindObjectsData* value);

typedef struct _FindObjectsEnvelope
{
    uint32_t Rv;
    FindObjectsData* Data;
} FindObjectsEnvelope;

int FindObjectsEnvelope_Serialize(cmp_ctx_t* ctx, FindObjectsEnvelope* value);
int FindObjectsEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsEnvelope* value);
int FindObjectsEnvelope_Release(FindObjectsEnvelope* value);

typedef struct _FindObjectsFinalRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
} FindObjectsFinalRequest;

int FindObjectsFinalRequest_Serialize(cmp_ctx_t* ctx, FindObjectsFinalRequest* value);
int FindObjectsFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsFinalRequest* value);
int FindObjectsFinalRequest_Release(FindObjectsFinalRequest* value);

typedef struct _FindObjectsFinalEnvelope
{
    uint32_t Rv;
} FindObjectsFinalEnvelope;

int FindObjectsFinalEnvelope_Serialize(cmp_ctx_t* ctx, FindObjectsFinalEnvelope* value);
int FindObjectsFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, FindObjectsFinalEnvelope* value);
int FindObjectsFinalEnvelope_Release(FindObjectsFinalEnvelope* value);

typedef struct _GetObjectSizeRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t ObjectHandle;
} GetObjectSizeRequest;

int GetObjectSizeRequest_Serialize(cmp_ctx_t* ctx, GetObjectSizeRequest* value);
int GetObjectSizeRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetObjectSizeRequest* value);
int GetObjectSizeRequest_Release(GetObjectSizeRequest* value);

typedef struct _GetObjectSizeEnvelope
{
    uint32_t Rv;
    CkSpecialUint* Data;
} GetObjectSizeEnvelope;

int GetObjectSizeEnvelope_Serialize(cmp_ctx_t* ctx, GetObjectSizeEnvelope* value);
int GetObjectSizeEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetObjectSizeEnvelope* value);
int GetObjectSizeEnvelope_Release(GetObjectSizeEnvelope* value);

typedef struct _GetAttributeInputValues
{
    uint32_t AttributeType;
    bool IsValuePtrSet;
    uint32_t ValueLen;
} GetAttributeInputValues;

int GetAttributeInputValues_Serialize(cmp_ctx_t* ctx, GetAttributeInputValues* value);
int GetAttributeInputValues_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetAttributeInputValues* value);
int GetAttributeInputValues_Release(GetAttributeInputValues* value);

typedef struct _GetAttributeValueRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t ObjectHandle;
    ArrayOfGetAttributeInputValues InTemplate;
} GetAttributeValueRequest;

int GetAttributeValueRequest_Serialize(cmp_ctx_t* ctx, GetAttributeValueRequest* value);
int GetAttributeValueRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetAttributeValueRequest* value);
int GetAttributeValueRequest_Release(GetAttributeValueRequest* value);

typedef struct _GetAttributeOutValue
{
    CkSpecialUint ValueLen;
    int32_t ValueType;
    uint32_t ValueUint;
    bool ValueBool;
    Binary ValueBytes;
    char* ValueCkDate;
} GetAttributeOutValue;

int GetAttributeOutValue_Serialize(cmp_ctx_t* ctx, GetAttributeOutValue* value);
int GetAttributeOutValue_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetAttributeOutValue* value);
int GetAttributeOutValue_Release(GetAttributeOutValue* value);

typedef struct _GetAttributeOutValues
{
    ArrayOfGetAttributeOutValue OutTemplate;
} GetAttributeOutValues;

int GetAttributeOutValues_Serialize(cmp_ctx_t* ctx, GetAttributeOutValues* value);
int GetAttributeOutValues_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetAttributeOutValues* value);
int GetAttributeOutValues_Release(GetAttributeOutValues* value);

typedef struct _GetAttributeValueEnvelope
{
    uint32_t Rv;
    GetAttributeOutValues* Data;
} GetAttributeValueEnvelope;

int GetAttributeValueEnvelope_Serialize(cmp_ctx_t* ctx, GetAttributeValueEnvelope* value);
int GetAttributeValueEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GetAttributeValueEnvelope* value);
int GetAttributeValueEnvelope_Release(GetAttributeValueEnvelope* value);

typedef struct _GenerateKeyPairRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    ArrayOfAttrValueFromNative PublicKeyTemplate;
    ArrayOfAttrValueFromNative PrivateKeyTemplate;
} GenerateKeyPairRequest;

int GenerateKeyPairRequest_Serialize(cmp_ctx_t* ctx, GenerateKeyPairRequest* value);
int GenerateKeyPairRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateKeyPairRequest* value);
int GenerateKeyPairRequest_Release(GenerateKeyPairRequest* value);

typedef struct _GenerateKeyPairData
{
    uint32_t PublicKeyHandle;
    uint32_t PrivateKeyHandle;
} GenerateKeyPairData;

int GenerateKeyPairData_Serialize(cmp_ctx_t* ctx, GenerateKeyPairData* value);
int GenerateKeyPairData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateKeyPairData* value);
int GenerateKeyPairData_Release(GenerateKeyPairData* value);

typedef struct _GenerateKeyPairEnvelope
{
    uint32_t Rv;
    GenerateKeyPairData* Data;
} GenerateKeyPairEnvelope;

int GenerateKeyPairEnvelope_Serialize(cmp_ctx_t* ctx, GenerateKeyPairEnvelope* value);
int GenerateKeyPairEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateKeyPairEnvelope* value);
int GenerateKeyPairEnvelope_Release(GenerateKeyPairEnvelope* value);

typedef struct _SignInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t KeyObjectHandle;
} SignInitRequest;

int SignInitRequest_Serialize(cmp_ctx_t* ctx, SignInitRequest* value);
int SignInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignInitRequest* value);
int SignInitRequest_Release(SignInitRequest* value);

typedef struct _SignInitEnvelope
{
    uint32_t Rv;
} SignInitEnvelope;

int SignInitEnvelope_Serialize(cmp_ctx_t* ctx, SignInitEnvelope* value);
int SignInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignInitEnvelope* value);
int SignInitEnvelope_Release(SignInitEnvelope* value);

typedef struct _SignRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
    bool IsSignaturePtrSet;
    uint32_t PullSignatureLen;
} SignRequest;

int SignRequest_Serialize(cmp_ctx_t* ctx, SignRequest* value);
int SignRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignRequest* value);
int SignRequest_Release(SignRequest* value);

typedef struct _SignatureData
{
    uint32_t PullSignatureLen;
    Binary Signature;
} SignatureData;

int SignatureData_Serialize(cmp_ctx_t* ctx, SignatureData* value);
int SignatureData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignatureData* value);
int SignatureData_Release(SignatureData* value);

typedef struct _SignEnvelope
{
    uint32_t Rv;
    SignatureData* Data;
} SignEnvelope;

int SignEnvelope_Serialize(cmp_ctx_t* ctx, SignEnvelope* value);
int SignEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignEnvelope* value);
int SignEnvelope_Release(SignEnvelope* value);

typedef struct _SignUpdateRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
} SignUpdateRequest;

int SignUpdateRequest_Serialize(cmp_ctx_t* ctx, SignUpdateRequest* value);
int SignUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignUpdateRequest* value);
int SignUpdateRequest_Release(SignUpdateRequest* value);

typedef struct _SignUpdateEnvelope
{
    uint32_t Rv;
} SignUpdateEnvelope;

int SignUpdateEnvelope_Serialize(cmp_ctx_t* ctx, SignUpdateEnvelope* value);
int SignUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignUpdateEnvelope* value);
int SignUpdateEnvelope_Release(SignUpdateEnvelope* value);

typedef struct _SignFinalRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    bool IsSignaturePtrSet;
    uint32_t PullSignatureLen;
} SignFinalRequest;

int SignFinalRequest_Serialize(cmp_ctx_t* ctx, SignFinalRequest* value);
int SignFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignFinalRequest* value);
int SignFinalRequest_Release(SignFinalRequest* value);

typedef struct _SignFinalEnvelope
{
    uint32_t Rv;
    SignatureData* Data;
} SignFinalEnvelope;

int SignFinalEnvelope_Serialize(cmp_ctx_t* ctx, SignFinalEnvelope* value);
int SignFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignFinalEnvelope* value);
int SignFinalEnvelope_Release(SignFinalEnvelope* value);

typedef struct _VerifyInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t KeyObjectHandle;
} VerifyInitRequest;

int VerifyInitRequest_Serialize(cmp_ctx_t* ctx, VerifyInitRequest* value);
int VerifyInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyInitRequest* value);
int VerifyInitRequest_Release(VerifyInitRequest* value);

typedef struct _VerifyInitEnvelope
{
    uint32_t Rv;
} VerifyInitEnvelope;

int VerifyInitEnvelope_Serialize(cmp_ctx_t* ctx, VerifyInitEnvelope* value);
int VerifyInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyInitEnvelope* value);
int VerifyInitEnvelope_Release(VerifyInitEnvelope* value);

typedef struct _VerifyRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
    Binary Signature;
} VerifyRequest;

int VerifyRequest_Serialize(cmp_ctx_t* ctx, VerifyRequest* value);
int VerifyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyRequest* value);
int VerifyRequest_Release(VerifyRequest* value);

typedef struct _VerifyEnvelope
{
    uint32_t Rv;
} VerifyEnvelope;

int VerifyEnvelope_Serialize(cmp_ctx_t* ctx, VerifyEnvelope* value);
int VerifyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyEnvelope* value);
int VerifyEnvelope_Release(VerifyEnvelope* value);

typedef struct _VerifyUpdateRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
} VerifyUpdateRequest;

int VerifyUpdateRequest_Serialize(cmp_ctx_t* ctx, VerifyUpdateRequest* value);
int VerifyUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyUpdateRequest* value);
int VerifyUpdateRequest_Release(VerifyUpdateRequest* value);

typedef struct _VerifyUpdateEnvelope
{
    uint32_t Rv;
} VerifyUpdateEnvelope;

int VerifyUpdateEnvelope_Serialize(cmp_ctx_t* ctx, VerifyUpdateEnvelope* value);
int VerifyUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyUpdateEnvelope* value);
int VerifyUpdateEnvelope_Release(VerifyUpdateEnvelope* value);

typedef struct _VerifyFinalRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Signature;
} VerifyFinalRequest;

int VerifyFinalRequest_Serialize(cmp_ctx_t* ctx, VerifyFinalRequest* value);
int VerifyFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyFinalRequest* value);
int VerifyFinalRequest_Release(VerifyFinalRequest* value);

typedef struct _VerifyFinalEnvelope
{
    uint32_t Rv;
} VerifyFinalEnvelope;

int VerifyFinalEnvelope_Serialize(cmp_ctx_t* ctx, VerifyFinalEnvelope* value);
int VerifyFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyFinalEnvelope* value);
int VerifyFinalEnvelope_Release(VerifyFinalEnvelope* value);

typedef struct _GenerateKeyRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    ArrayOfAttrValueFromNative Template;
} GenerateKeyRequest;

int GenerateKeyRequest_Serialize(cmp_ctx_t* ctx, GenerateKeyRequest* value);
int GenerateKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateKeyRequest* value);
int GenerateKeyRequest_Release(GenerateKeyRequest* value);

typedef struct _GenerateKeyData
{
    uint32_t KeyHandle;
} GenerateKeyData;

int GenerateKeyData_Serialize(cmp_ctx_t* ctx, GenerateKeyData* value);
int GenerateKeyData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateKeyData* value);
int GenerateKeyData_Release(GenerateKeyData* value);

typedef struct _GenerateKeyEnvelope
{
    uint32_t Rv;
    GenerateKeyData* Data;
} GenerateKeyEnvelope;

int GenerateKeyEnvelope_Serialize(cmp_ctx_t* ctx, GenerateKeyEnvelope* value);
int GenerateKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, GenerateKeyEnvelope* value);
int GenerateKeyEnvelope_Release(GenerateKeyEnvelope* value);

typedef struct _DeriveKeyRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t BaseKeyHandle;
    ArrayOfAttrValueFromNative Template;
} DeriveKeyRequest;

int DeriveKeyRequest_Serialize(cmp_ctx_t* ctx, DeriveKeyRequest* value);
int DeriveKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DeriveKeyRequest* value);
int DeriveKeyRequest_Release(DeriveKeyRequest* value);

typedef struct _DeriveKeyData
{
    uint32_t KeyHandle;
} DeriveKeyData;

int DeriveKeyData_Serialize(cmp_ctx_t* ctx, DeriveKeyData* value);
int DeriveKeyData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DeriveKeyData* value);
int DeriveKeyData_Release(DeriveKeyData* value);

typedef struct _DeriveKeyEnvelope
{
    uint32_t Rv;
    DeriveKeyData* Data;
} DeriveKeyEnvelope;

int DeriveKeyEnvelope_Serialize(cmp_ctx_t* ctx, DeriveKeyEnvelope* value);
int DeriveKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DeriveKeyEnvelope* value);
int DeriveKeyEnvelope_Release(DeriveKeyEnvelope* value);

typedef struct _EncryptInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t KeyObjectHandle;
} EncryptInitRequest;

int EncryptInitRequest_Serialize(cmp_ctx_t* ctx, EncryptInitRequest* value);
int EncryptInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptInitRequest* value);
int EncryptInitRequest_Release(EncryptInitRequest* value);

typedef struct _EncryptInitEnvelope
{
    uint32_t Rv;
} EncryptInitEnvelope;

int EncryptInitEnvelope_Serialize(cmp_ctx_t* ctx, EncryptInitEnvelope* value);
int EncryptInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptInitEnvelope* value);
int EncryptInitEnvelope_Release(EncryptInitEnvelope* value);

typedef struct _EncryptRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
    bool IsEncryptedDataPtrSet;
    uint32_t EncryptedDataLen;
} EncryptRequest;

int EncryptRequest_Serialize(cmp_ctx_t* ctx, EncryptRequest* value);
int EncryptRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptRequest* value);
int EncryptRequest_Release(EncryptRequest* value);

typedef struct _EncryptData
{
    uint32_t PullEncryptedDataLen;
    Binary EncryptedData;
} EncryptData;

int EncryptData_Serialize(cmp_ctx_t* ctx, EncryptData* value);
int EncryptData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptData* value);
int EncryptData_Release(EncryptData* value);

typedef struct _EncryptEnvelope
{
    uint32_t Rv;
    EncryptData* Data;
} EncryptEnvelope;

int EncryptEnvelope_Serialize(cmp_ctx_t* ctx, EncryptEnvelope* value);
int EncryptEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptEnvelope* value);
int EncryptEnvelope_Release(EncryptEnvelope* value);

typedef struct _EncryptUpdateRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary PartData;
    bool IsEncryptedDataPtrSet;
    uint32_t EncryptedDataLen;
} EncryptUpdateRequest;

int EncryptUpdateRequest_Serialize(cmp_ctx_t* ctx, EncryptUpdateRequest* value);
int EncryptUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptUpdateRequest* value);
int EncryptUpdateRequest_Release(EncryptUpdateRequest* value);

typedef struct _EncryptUpdateEnvelope
{
    uint32_t Rv;
    EncryptData* Data;
} EncryptUpdateEnvelope;

int EncryptUpdateEnvelope_Serialize(cmp_ctx_t* ctx, EncryptUpdateEnvelope* value);
int EncryptUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptUpdateEnvelope* value);
int EncryptUpdateEnvelope_Release(EncryptUpdateEnvelope* value);

typedef struct _EncryptFinalRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    bool IsEncryptedDataPtrSet;
    uint32_t EncryptedDataLen;
} EncryptFinalRequest;

int EncryptFinalRequest_Serialize(cmp_ctx_t* ctx, EncryptFinalRequest* value);
int EncryptFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptFinalRequest* value);
int EncryptFinalRequest_Release(EncryptFinalRequest* value);

typedef struct _EncryptFinalEnvelope
{
    uint32_t Rv;
    EncryptData* Data;
} EncryptFinalEnvelope;

int EncryptFinalEnvelope_Serialize(cmp_ctx_t* ctx, EncryptFinalEnvelope* value);
int EncryptFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, EncryptFinalEnvelope* value);
int EncryptFinalEnvelope_Release(EncryptFinalEnvelope* value);

typedef struct _DecryptInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t KeyObjectHandle;
} DecryptInitRequest;

int DecryptInitRequest_Serialize(cmp_ctx_t* ctx, DecryptInitRequest* value);
int DecryptInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptInitRequest* value);
int DecryptInitRequest_Release(DecryptInitRequest* value);

typedef struct _DecryptInitEnvelope
{
    uint32_t Rv;
} DecryptInitEnvelope;

int DecryptInitEnvelope_Serialize(cmp_ctx_t* ctx, DecryptInitEnvelope* value);
int DecryptInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptInitEnvelope* value);
int DecryptInitEnvelope_Release(DecryptInitEnvelope* value);

typedef struct _DecryptRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary EncryptedData;
    bool IsDataPtrSet;
    uint32_t PullDataLen;
} DecryptRequest;

int DecryptRequest_Serialize(cmp_ctx_t* ctx, DecryptRequest* value);
int DecryptRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptRequest* value);
int DecryptRequest_Release(DecryptRequest* value);

typedef struct _DecryptData
{
    Binary Data;
    uint32_t PullDataLen;
} DecryptData;

int DecryptData_Serialize(cmp_ctx_t* ctx, DecryptData* value);
int DecryptData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptData* value);
int DecryptData_Release(DecryptData* value);

typedef struct _DecryptEnvelope
{
    uint32_t Rv;
    DecryptData* Data;
} DecryptEnvelope;

int DecryptEnvelope_Serialize(cmp_ctx_t* ctx, DecryptEnvelope* value);
int DecryptEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptEnvelope* value);
int DecryptEnvelope_Release(DecryptEnvelope* value);

typedef struct _DecryptUpdateRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary EncryptedData;
    bool IsDataPtrSet;
    uint32_t PullDataLen;
} DecryptUpdateRequest;

int DecryptUpdateRequest_Serialize(cmp_ctx_t* ctx, DecryptUpdateRequest* value);
int DecryptUpdateRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptUpdateRequest* value);
int DecryptUpdateRequest_Release(DecryptUpdateRequest* value);

typedef struct _DecryptUpdateEnvelope
{
    uint32_t Rv;
    DecryptData* Data;
} DecryptUpdateEnvelope;

int DecryptUpdateEnvelope_Serialize(cmp_ctx_t* ctx, DecryptUpdateEnvelope* value);
int DecryptUpdateEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptUpdateEnvelope* value);
int DecryptUpdateEnvelope_Release(DecryptUpdateEnvelope* value);

typedef struct _DecryptFinalRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    bool IsDataPtrSet;
    uint32_t PullDataLen;
} DecryptFinalRequest;

int DecryptFinalRequest_Serialize(cmp_ctx_t* ctx, DecryptFinalRequest* value);
int DecryptFinalRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptFinalRequest* value);
int DecryptFinalRequest_Release(DecryptFinalRequest* value);

typedef struct _DecryptFinalEnvelope
{
    uint32_t Rv;
    DecryptData* Data;
} DecryptFinalEnvelope;

int DecryptFinalEnvelope_Serialize(cmp_ctx_t* ctx, DecryptFinalEnvelope* value);
int DecryptFinalEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, DecryptFinalEnvelope* value);
int DecryptFinalEnvelope_Release(DecryptFinalEnvelope* value);

typedef struct _WrapKeyRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t WrappingKeyHandle;
    uint32_t KeyHandle;
    bool IsPtrWrappedKeySet;
    uint32_t PulWrappedKeyLen;
} WrapKeyRequest;

int WrapKeyRequest_Serialize(cmp_ctx_t* ctx, WrapKeyRequest* value);
int WrapKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, WrapKeyRequest* value);
int WrapKeyRequest_Release(WrapKeyRequest* value);

typedef struct _WrapKeyData
{
    Binary WrappedKeyData;
    uint32_t PulWrappedKeyLen;
} WrapKeyData;

int WrapKeyData_Serialize(cmp_ctx_t* ctx, WrapKeyData* value);
int WrapKeyData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, WrapKeyData* value);
int WrapKeyData_Release(WrapKeyData* value);

typedef struct _WrapKeyEnvelope
{
    uint32_t Rv;
    WrapKeyData* Data;
} WrapKeyEnvelope;

int WrapKeyEnvelope_Serialize(cmp_ctx_t* ctx, WrapKeyEnvelope* value);
int WrapKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, WrapKeyEnvelope* value);
int WrapKeyEnvelope_Release(WrapKeyEnvelope* value);

typedef struct _UnwrapKeyRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t UnwrappingKeyHandle;
    Binary WrappedKeyData;
    ArrayOfAttrValueFromNative Template;
} UnwrapKeyRequest;

int UnwrapKeyRequest_Serialize(cmp_ctx_t* ctx, UnwrapKeyRequest* value);
int UnwrapKeyRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, UnwrapKeyRequest* value);
int UnwrapKeyRequest_Release(UnwrapKeyRequest* value);

typedef struct _UnwrapKeyData
{
    uint32_t KeyHandle;
} UnwrapKeyData;

int UnwrapKeyData_Serialize(cmp_ctx_t* ctx, UnwrapKeyData* value);
int UnwrapKeyData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, UnwrapKeyData* value);
int UnwrapKeyData_Release(UnwrapKeyData* value);

typedef struct _UnwrapKeyEnvelope
{
    uint32_t Rv;
    UnwrapKeyData* Data;
} UnwrapKeyEnvelope;

int UnwrapKeyEnvelope_Serialize(cmp_ctx_t* ctx, UnwrapKeyEnvelope* value);
int UnwrapKeyEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, UnwrapKeyEnvelope* value);
int UnwrapKeyEnvelope_Release(UnwrapKeyEnvelope* value);

typedef struct _SetAttributeValueRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t ObjectHandle;
    ArrayOfAttrValueFromNative Template;
} SetAttributeValueRequest;

int SetAttributeValueRequest_Serialize(cmp_ctx_t* ctx, SetAttributeValueRequest* value);
int SetAttributeValueRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SetAttributeValueRequest* value);
int SetAttributeValueRequest_Release(SetAttributeValueRequest* value);

typedef struct _SetAttributeValueEnvelope
{
    uint32_t Rv;
} SetAttributeValueEnvelope;

int SetAttributeValueEnvelope_Serialize(cmp_ctx_t* ctx, SetAttributeValueEnvelope* value);
int SetAttributeValueEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SetAttributeValueEnvelope* value);
int SetAttributeValueEnvelope_Release(SetAttributeValueEnvelope* value);

typedef struct _CopyObjectRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    uint32_t ObjectHandle;
    ArrayOfAttrValueFromNative Template;
} CopyObjectRequest;

int CopyObjectRequest_Serialize(cmp_ctx_t* ctx, CopyObjectRequest* value);
int CopyObjectRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CopyObjectRequest* value);
int CopyObjectRequest_Release(CopyObjectRequest* value);

typedef struct _CopyObjectData
{
    uint32_t ObjectHandle;
} CopyObjectData;

int CopyObjectData_Serialize(cmp_ctx_t* ctx, CopyObjectData* value);
int CopyObjectData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CopyObjectData* value);
int CopyObjectData_Release(CopyObjectData* value);

typedef struct _CopyObjectEnvelope
{
    uint32_t Rv;
    CopyObjectData* Data;
} CopyObjectEnvelope;

int CopyObjectEnvelope_Serialize(cmp_ctx_t* ctx, CopyObjectEnvelope* value);
int CopyObjectEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CopyObjectEnvelope* value);
int CopyObjectEnvelope_Release(CopyObjectEnvelope* value);

typedef struct _WaitForSlotEventRequest
{
    AppIdentification AppId;
    uint32_t Flags;
    bool IsSlotPtrSet;
    bool IsReservedPtrSet;
} WaitForSlotEventRequest;

int WaitForSlotEventRequest_Serialize(cmp_ctx_t* ctx, WaitForSlotEventRequest* value);
int WaitForSlotEventRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, WaitForSlotEventRequest* value);
int WaitForSlotEventRequest_Release(WaitForSlotEventRequest* value);

typedef struct _WaitForSlotEventEnvelope
{
    uint32_t Rv;
    WaitForSlotEventData* Data;
} WaitForSlotEventEnvelope;

int WaitForSlotEventEnvelope_Serialize(cmp_ctx_t* ctx, WaitForSlotEventEnvelope* value);
int WaitForSlotEventEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, WaitForSlotEventEnvelope* value);
int WaitForSlotEventEnvelope_Release(WaitForSlotEventEnvelope* value);

typedef struct _WaitForSlotEventData
{
    uint32_t SlotId;
} WaitForSlotEventData;

int WaitForSlotEventData_Serialize(cmp_ctx_t* ctx, WaitForSlotEventData* value);
int WaitForSlotEventData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, WaitForSlotEventData* value);
int WaitForSlotEventData_Release(WaitForSlotEventData* value);

typedef struct _SignRecoverInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t KeyObjectHandle;
} SignRecoverInitRequest;

int SignRecoverInitRequest_Serialize(cmp_ctx_t* ctx, SignRecoverInitRequest* value);
int SignRecoverInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignRecoverInitRequest* value);
int SignRecoverInitRequest_Release(SignRecoverInitRequest* value);

typedef struct _SignRecoverInitEnvelope
{
    uint32_t Rv;
} SignRecoverInitEnvelope;

int SignRecoverInitEnvelope_Serialize(cmp_ctx_t* ctx, SignRecoverInitEnvelope* value);
int SignRecoverInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignRecoverInitEnvelope* value);
int SignRecoverInitEnvelope_Release(SignRecoverInitEnvelope* value);

typedef struct _SignRecoverRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Data;
    bool IsSignaturePtrSet;
    uint32_t PullSignatureLen;
} SignRecoverRequest;

int SignRecoverRequest_Serialize(cmp_ctx_t* ctx, SignRecoverRequest* value);
int SignRecoverRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignRecoverRequest* value);
int SignRecoverRequest_Release(SignRecoverRequest* value);

typedef struct _SignRecoverData
{
    uint32_t PullSignatureLen;
    Binary Signature;
} SignRecoverData;

int SignRecoverData_Serialize(cmp_ctx_t* ctx, SignRecoverData* value);
int SignRecoverData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignRecoverData* value);
int SignRecoverData_Release(SignRecoverData* value);

typedef struct _SignRecoverEnvelope
{
    uint32_t Rv;
    SignRecoverData* Data;
} SignRecoverEnvelope;

int SignRecoverEnvelope_Serialize(cmp_ctx_t* ctx, SignRecoverEnvelope* value);
int SignRecoverEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, SignRecoverEnvelope* value);
int SignRecoverEnvelope_Release(SignRecoverEnvelope* value);

typedef struct _VerifyRecoverInitRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    MechanismValue Mechanism;
    uint32_t KeyObjectHandle;
} VerifyRecoverInitRequest;

int VerifyRecoverInitRequest_Serialize(cmp_ctx_t* ctx, VerifyRecoverInitRequest* value);
int VerifyRecoverInitRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyRecoverInitRequest* value);
int VerifyRecoverInitRequest_Release(VerifyRecoverInitRequest* value);

typedef struct _VerifyRecoverInitEnvelope
{
    uint32_t Rv;
} VerifyRecoverInitEnvelope;

int VerifyRecoverInitEnvelope_Serialize(cmp_ctx_t* ctx, VerifyRecoverInitEnvelope* value);
int VerifyRecoverInitEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyRecoverInitEnvelope* value);
int VerifyRecoverInitEnvelope_Release(VerifyRecoverInitEnvelope* value);

typedef struct _VerifyRecoverRequest
{
    AppIdentification AppId;
    uint32_t SessionId;
    Binary Signature;
    bool IsPtrDataSet;
    uint32_t PulDataLen;
} VerifyRecoverRequest;

int VerifyRecoverRequest_Serialize(cmp_ctx_t* ctx, VerifyRecoverRequest* value);
int VerifyRecoverRequest_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyRecoverRequest* value);
int VerifyRecoverRequest_Release(VerifyRecoverRequest* value);

typedef struct _VerifyRecoverData
{
    uint32_t PulDataLen;
    Binary Data;
} VerifyRecoverData;

int VerifyRecoverData_Serialize(cmp_ctx_t* ctx, VerifyRecoverData* value);
int VerifyRecoverData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyRecoverData* value);
int VerifyRecoverData_Release(VerifyRecoverData* value);

typedef struct _VerifyRecoverEnvelope
{
    uint32_t Rv;
    VerifyRecoverData* Data;
} VerifyRecoverEnvelope;

int VerifyRecoverEnvelope_Serialize(cmp_ctx_t* ctx, VerifyRecoverEnvelope* value);
int VerifyRecoverEnvelope_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, VerifyRecoverEnvelope* value);
int VerifyRecoverEnvelope_Release(VerifyRecoverEnvelope* value);

typedef struct _CkP_MacGeneralParams
{
    uint32_t Value;
} CkP_MacGeneralParams;

int CkP_MacGeneralParams_Serialize(cmp_ctx_t* ctx, CkP_MacGeneralParams* value);
int CkP_MacGeneralParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkP_MacGeneralParams* value);
int CkP_MacGeneralParams_Release(CkP_MacGeneralParams* value);

typedef struct _CkP_ExtractParams
{
    uint32_t Value;
} CkP_ExtractParams;

int CkP_ExtractParams_Serialize(cmp_ctx_t* ctx, CkP_ExtractParams* value);
int CkP_ExtractParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkP_ExtractParams* value);
int CkP_ExtractParams_Release(CkP_ExtractParams* value);

typedef struct _CkP_RsaPkcsPssParams
{
    uint32_t HashAlg;
    uint32_t Mgf;
    uint32_t SLen;
} CkP_RsaPkcsPssParams;

int CkP_RsaPkcsPssParams_Serialize(cmp_ctx_t* ctx, CkP_RsaPkcsPssParams* value);
int CkP_RsaPkcsPssParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkP_RsaPkcsPssParams* value);
int CkP_RsaPkcsPssParams_Release(CkP_RsaPkcsPssParams* value);

typedef struct _CkP_RawDataParams
{
    Binary Value;
} CkP_RawDataParams;

int CkP_RawDataParams_Serialize(cmp_ctx_t* ctx, CkP_RawDataParams* value);
int CkP_RawDataParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkP_RawDataParams* value);
int CkP_RawDataParams_Release(CkP_RawDataParams* value);

typedef struct _CkP_KeyDerivationStringData
{
    Binary Data;
    uint32_t Len;
} CkP_KeyDerivationStringData;

int CkP_KeyDerivationStringData_Serialize(cmp_ctx_t* ctx, CkP_KeyDerivationStringData* value);
int CkP_KeyDerivationStringData_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkP_KeyDerivationStringData* value);
int CkP_KeyDerivationStringData_Release(CkP_KeyDerivationStringData* value);

typedef struct _CkP_CkObjectHandle
{
    uint32_t Handle;
} CkP_CkObjectHandle;

int CkP_CkObjectHandle_Serialize(cmp_ctx_t* ctx, CkP_CkObjectHandle* value);
int CkP_CkObjectHandle_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, CkP_CkObjectHandle* value);
int CkP_CkObjectHandle_Release(CkP_CkObjectHandle* value);

typedef struct _Ckp_CkEcdh1DeriveParams
{
    uint32_t Kdf;
    Binary* SharedData;
    Binary PublicData;
} Ckp_CkEcdh1DeriveParams;

int Ckp_CkEcdh1DeriveParams_Serialize(cmp_ctx_t* ctx, Ckp_CkEcdh1DeriveParams* value);
int Ckp_CkEcdh1DeriveParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkEcdh1DeriveParams* value);
int Ckp_CkEcdh1DeriveParams_Release(Ckp_CkEcdh1DeriveParams* value);

typedef struct _Ckp_CkGcmParams
{
    Binary* Iv;
    uint32_t IvBits;
    Binary* Aad;
    uint32_t TagBits;
} Ckp_CkGcmParams;

int Ckp_CkGcmParams_Serialize(cmp_ctx_t* ctx, Ckp_CkGcmParams* value);
int Ckp_CkGcmParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkGcmParams* value);
int Ckp_CkGcmParams_Release(Ckp_CkGcmParams* value);

typedef struct _Ckp_CkCcmParams
{
    uint32_t DataLen;
    Binary* Nonce;
    Binary* Aad;
    uint32_t MacLen;
} Ckp_CkCcmParams;

int Ckp_CkCcmParams_Serialize(cmp_ctx_t* ctx, Ckp_CkCcmParams* value);
int Ckp_CkCcmParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkCcmParams* value);
int Ckp_CkCcmParams_Release(Ckp_CkCcmParams* value);

typedef struct _Ckp_CkRsaPkcsOaepParams
{
    uint32_t HashAlg;
    uint32_t Mgf;
    uint32_t Source;
    Binary* SourceData;
} Ckp_CkRsaPkcsOaepParams;

int Ckp_CkRsaPkcsOaepParams_Serialize(cmp_ctx_t* ctx, Ckp_CkRsaPkcsOaepParams* value);
int Ckp_CkRsaPkcsOaepParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkRsaPkcsOaepParams* value);
int Ckp_CkRsaPkcsOaepParams_Release(Ckp_CkRsaPkcsOaepParams* value);

typedef struct _Ckp_CkAesCbcEnryptDataParams
{
    Binary Iv;
    Binary Data;
} Ckp_CkAesCbcEnryptDataParams;

int Ckp_CkAesCbcEnryptDataParams_Serialize(cmp_ctx_t* ctx, Ckp_CkAesCbcEnryptDataParams* value);
int Ckp_CkAesCbcEnryptDataParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkAesCbcEnryptDataParams* value);
int Ckp_CkAesCbcEnryptDataParams_Release(Ckp_CkAesCbcEnryptDataParams* value);

typedef struct _Ckp_CkChaCha20Params
{
    uint32_t BlockCounterLower;
    uint32_t BlockCounterUpper;
    bool BlockCounterIsSet;
    uint32_t BlockCounterBits;
    Binary Nonce;
} Ckp_CkChaCha20Params;

int Ckp_CkChaCha20Params_Serialize(cmp_ctx_t* ctx, Ckp_CkChaCha20Params* value);
int Ckp_CkChaCha20Params_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkChaCha20Params* value);
int Ckp_CkChaCha20Params_Release(Ckp_CkChaCha20Params* value);

typedef struct _Ckp_CkSalsa20Params
{
    uint64_t BlockCounter;
    bool BlockCounterIsSet;
    Binary Nonce;
} Ckp_CkSalsa20Params;

int Ckp_CkSalsa20Params_Serialize(cmp_ctx_t* ctx, Ckp_CkSalsa20Params* value);
int Ckp_CkSalsa20Params_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkSalsa20Params* value);
int Ckp_CkSalsa20Params_Release(Ckp_CkSalsa20Params* value);

typedef struct _Ckp_CkSalsa20ChaCha20Poly1305Params
{
    Binary Nonce;
    Binary* AadData;
} Ckp_CkSalsa20ChaCha20Poly1305Params;

int Ckp_CkSalsa20ChaCha20Poly1305Params_Serialize(cmp_ctx_t* ctx, Ckp_CkSalsa20ChaCha20Poly1305Params* value);
int Ckp_CkSalsa20ChaCha20Poly1305Params_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkSalsa20ChaCha20Poly1305Params* value);
int Ckp_CkSalsa20ChaCha20Poly1305Params_Release(Ckp_CkSalsa20ChaCha20Poly1305Params* value);

typedef struct _Ckp_CkEddsaParams
{
    bool PhFlag;
    Binary* ContextData;
} Ckp_CkEddsaParams;

int Ckp_CkEddsaParams_Serialize(cmp_ctx_t* ctx, Ckp_CkEddsaParams* value);
int Ckp_CkEddsaParams_Deserialize(cmp_ctx_t* ctx, const cmp_object_t* start_obj, Ckp_CkEddsaParams* value);
int Ckp_CkEddsaParams_Release(Ckp_CkEddsaParams* value);


typedef void* (*nmrpc_malloc_fn_t)(size_t size);
typedef void (*nmrpc_free_fn_t)(void* ptr);
typedef void* (*nmrpc_realloc_fn_t)(void* ptr, size_t new_size);

typedef int (*nmrpc_writerequest_fn_t)(void* user_ctx, void* request_data, size_t request_data_size);
typedef int (*nmrpc_flush_fn_t)(void* user_ctx);
typedef size_t (*nmrpc_readresponse_fn_t)(void* user_ctx, void* response_data, size_t response_data_size);
typedef int (*nmrpc_readclose_fn_t)(void* user_ctx);

typedef struct _nmrpc_global_context {
    void* user_ctx;

    nmrpc_writerequest_fn_t write;
    nmrpc_readresponse_fn_t read;
    nmrpc_flush_fn_t flush;
    nmrpc_readclose_fn_t close;

    char *tag;
} nmrpc_global_context_t;


int nmrpc_global_context_init(nmrpc_global_context_t* ctx, void* user_ctx, nmrpc_writerequest_fn_t write, nmrpc_readresponse_fn_t read, nmrpc_readclose_fn_t close, nmrpc_flush_fn_t flush);

int nmrpc_call_Ping(nmrpc_global_context_t* ctx, PingRequest* request, PingEnvelope* response);
int nmrpc_call_Initialize(nmrpc_global_context_t* ctx, InitializeRequest* request, InitializeEnvelope* response);
int nmrpc_call_Finalize(nmrpc_global_context_t* ctx, FinalizeRequest* request, FinalizeEnvelope* response);
int nmrpc_call_GetInfo(nmrpc_global_context_t* ctx, GetInfoRequest* request, GetInfoEnvelope* response);
int nmrpc_call_GetSlotList(nmrpc_global_context_t* ctx, GetSlotListRequest* request, GetSlotListEnvelope* response);
int nmrpc_call_GetSlotInfo(nmrpc_global_context_t* ctx, GetSlotInfoRequest* request, GetSlotInfoEnvelope* response);
int nmrpc_call_GetTokenInfo(nmrpc_global_context_t* ctx, GetTokenInfoRequest* request, GetTokenInfoEnvelope* response);
int nmrpc_call_GetMechanismList(nmrpc_global_context_t* ctx, GetMechanismListRequest* request, GetMechanismListEnvelope* response);
int nmrpc_call_GetMechanismInfo(nmrpc_global_context_t* ctx, GetMechanismInfoRequest* request, GetMechanismInfoEnvelope* response);
int nmrpc_call_SetPin(nmrpc_global_context_t* ctx, SetPinRequest* request, SetPinEnvelope* response);
int nmrpc_call_OpenSession(nmrpc_global_context_t* ctx, OpenSessionRequest* request, OpenSessionEnvelope* response);
int nmrpc_call_CloseSession(nmrpc_global_context_t* ctx, CloseSessionRequest* request, CloseSessionEnvelope* response);
int nmrpc_call_CloseAllSessions(nmrpc_global_context_t* ctx, CloseAllSessionsRequest* request, CloseAllSessionsEnvelope* response);
int nmrpc_call_GetSessionInfo(nmrpc_global_context_t* ctx, GetSessionInfoRequest* request, GetSessionInfoEnvelope* response);
int nmrpc_call_Login(nmrpc_global_context_t* ctx, LoginRequest* request, LoginEnvelope* response);
int nmrpc_call_Logout(nmrpc_global_context_t* ctx, LogoutRequest* request, LogoutEnvelope* response);
int nmrpc_call_SeedRandom(nmrpc_global_context_t* ctx, SeedRandomRequest* request, SeedRandomEnvelope* response);
int nmrpc_call_GenerateRandom(nmrpc_global_context_t* ctx, GenerateRandomRequest* request, GenerateRandomEnvelope* response);
int nmrpc_call_DigestInit(nmrpc_global_context_t* ctx, DigestInitRequest* request, DigestInitEnvelope* response);
int nmrpc_call_Digest(nmrpc_global_context_t* ctx, DigestRequest* request, DigestEnvelope* response);
int nmrpc_call_DigestUpdate(nmrpc_global_context_t* ctx, DigestUpdateRequest* request, DigestUpdateEnvelope* response);
int nmrpc_call_DigestKey(nmrpc_global_context_t* ctx, DigestKeyRequest* request, DigestKeyEnvelope* response);
int nmrpc_call_DigestFinal(nmrpc_global_context_t* ctx, DigestFinalRequest* request, DigestFinalEnvelope* response);
int nmrpc_call_CreateObject(nmrpc_global_context_t* ctx, CreateObjectRequest* request, CreateObjectEnvelope* response);
int nmrpc_call_DestroyObject(nmrpc_global_context_t* ctx, DestroyObjectRequest* request, DestroyObjectEnvelope* response);
int nmrpc_call_FindObjectsInit(nmrpc_global_context_t* ctx, FindObjectsInitRequest* request, FindObjectsInitEnvelope* response);
int nmrpc_call_FindObjects(nmrpc_global_context_t* ctx, FindObjectsRequest* request, FindObjectsEnvelope* response);
int nmrpc_call_FindObjectsFinal(nmrpc_global_context_t* ctx, FindObjectsFinalRequest* request, FindObjectsFinalEnvelope* response);
int nmrpc_call_GetObjectSize(nmrpc_global_context_t* ctx, GetObjectSizeRequest* request, GetObjectSizeEnvelope* response);
int nmrpc_call_GetAttributeValue(nmrpc_global_context_t* ctx, GetAttributeValueRequest* request, GetAttributeValueEnvelope* response);
int nmrpc_call_GenerateKeyPair(nmrpc_global_context_t* ctx, GenerateKeyPairRequest* request, GenerateKeyPairEnvelope* response);
int nmrpc_call_SetAttributeValue(nmrpc_global_context_t* ctx, SetAttributeValueRequest* request, SetAttributeValueEnvelope* response);
int nmrpc_call_CopyObject(nmrpc_global_context_t* ctx, CopyObjectRequest* request, CopyObjectEnvelope* response);
int nmrpc_call_SignInit(nmrpc_global_context_t* ctx, SignInitRequest* request, SignInitEnvelope* response);
int nmrpc_call_Sign(nmrpc_global_context_t* ctx, SignRequest* request, SignEnvelope* response);
int nmrpc_call_SignUpdate(nmrpc_global_context_t* ctx, SignUpdateRequest* request, SignUpdateEnvelope* response);
int nmrpc_call_SignFinal(nmrpc_global_context_t* ctx, SignFinalRequest* request, SignFinalEnvelope* response);
int nmrpc_call_VerifyInit(nmrpc_global_context_t* ctx, VerifyInitRequest* request, VerifyInitEnvelope* response);
int nmrpc_call_Verify(nmrpc_global_context_t* ctx, VerifyRequest* request, VerifyEnvelope* response);
int nmrpc_call_VerifyUpdate(nmrpc_global_context_t* ctx, VerifyUpdateRequest* request, VerifyUpdateEnvelope* response);
int nmrpc_call_VerifyFinal(nmrpc_global_context_t* ctx, VerifyFinalRequest* request, VerifyFinalEnvelope* response);
int nmrpc_call_GenerateKey(nmrpc_global_context_t* ctx, GenerateKeyRequest* request, GenerateKeyEnvelope* response);
int nmrpc_call_DeriveKey(nmrpc_global_context_t* ctx, DeriveKeyRequest* request, DeriveKeyEnvelope* response);
int nmrpc_call_EncryptInit(nmrpc_global_context_t* ctx, EncryptInitRequest* request, EncryptInitEnvelope* response);
int nmrpc_call_Encrypt(nmrpc_global_context_t* ctx, EncryptRequest* request, EncryptEnvelope* response);
int nmrpc_call_EncryptUpdate(nmrpc_global_context_t* ctx, EncryptUpdateRequest* request, EncryptUpdateEnvelope* response);
int nmrpc_call_EncryptFinal(nmrpc_global_context_t* ctx, EncryptFinalRequest* request, EncryptFinalEnvelope* response);
int nmrpc_call_DecryptInit(nmrpc_global_context_t* ctx, DecryptInitRequest* request, DecryptInitEnvelope* response);
int nmrpc_call_Decrypt(nmrpc_global_context_t* ctx, DecryptRequest* request, DecryptEnvelope* response);
int nmrpc_call_DecryptUpdate(nmrpc_global_context_t* ctx, DecryptUpdateRequest* request, DecryptUpdateEnvelope* response);
int nmrpc_call_DecryptFinal(nmrpc_global_context_t* ctx, DecryptFinalRequest* request, DecryptFinalEnvelope* response);
int nmrpc_call_WrapKey(nmrpc_global_context_t* ctx, WrapKeyRequest* request, WrapKeyEnvelope* response);
int nmrpc_call_UnwrapKey(nmrpc_global_context_t* ctx, UnwrapKeyRequest* request, UnwrapKeyEnvelope* response);
int nmrpc_call_WaitForSlotEvent(nmrpc_global_context_t* ctx, WaitForSlotEventRequest* request, WaitForSlotEventEnvelope* response);
int nmrpc_call_SignRecoverInit(nmrpc_global_context_t* ctx, SignRecoverInitRequest* request, SignRecoverInitEnvelope* response);
int nmrpc_call_SignRecover(nmrpc_global_context_t* ctx, SignRecoverRequest* request, SignRecoverEnvelope* response);
int nmrpc_call_VerifyRecoverInit(nmrpc_global_context_t* ctx, VerifyRecoverInitRequest* request, VerifyRecoverInitEnvelope* response);
int nmrpc_call_VerifyRecover(nmrpc_global_context_t* ctx, VerifyRecoverRequest* request, VerifyRecoverEnvelope* response);

#endif // NMRPC_rpc
