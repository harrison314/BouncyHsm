#include <stdlib.h>
#include <ctype.h>
#include "bouncy-pkcs11.h"

#include "rpc/rpc.h"
#include "rpc/tcpTransport.h"
#include "globalContext.h"
#include "timer.h"
#include "logger.h"
#include "platformHelper.h"

#ifndef _WIN32
#include <unistd.h>
#endif

// https://github.com/Pkcs11Interop/pkcs11-mock/blob/master/src/pkcs11-mock.c
// https://docs.oasis-open.org/pkcs11/pkcs11-base/v2.40/os/pkcs11-base-v2.40-os.html


void SetPaddedStrSafe(CK_UTF8CHAR* destination, size_t destinationSize, const char* src)
{
    size_t copySize = strlen(src);
    if (copySize > destinationSize)
    {
        copySize = destinationSize;
    }

    memset((void*)destination, ' ', destinationSize);
    memcpy((void*)destination, src, copySize);
}

CK_ULONG ConvertCkSpecialUint(CkSpecialUint value)
{
    if (value.UnavailableInformation)
    {
        return CK_UNAVAILABLE_INFORMATION;
    }

    if (value.EffectivelyInfinite)
    {
        return CK_EFFECTIVELY_INFINITE;
    }

    if (value.InformationSensitive)
    {
        return CKR_INFORMATION_SENSITIVE;
    }

    return (CK_ULONG)value.Value;
}

#define AttrValueFromNative_TypeHint_Binary 0x01
#define AttrValueFromNative_TypeHint_Bool 0x02
#define AttrValueFromNative_TypeHint_CkUlong 0x04
#define AttrValueFromNative_TypeHint_CkDate 0x08

#define AttrValueToNative_TypeHint_Binary 0x01
#define AttrValueToNative_TypeHint_Bool 0x02
#define AttrValueToNative_TypeHint_CkUlong 0x04
#define AttrValueToNative_TypeHint_CkDate 0x08

AttrValueFromNative* ConvertToAttrValueFromNative(CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    size_t allocCount = (ulCount > 0) ? sizeof(AttrValueFromNative) * ulCount : sizeof(AttrValueFromNative);

    AttrValueFromNative* ptr = (AttrValueFromNative*)malloc(allocCount);
    if (NULL == ptr)
    {
        return NULL;
    }

    CK_ULONG i;
    for (i = 0; i < ulCount; i++)
    {
        ptr[i].ValueBool = false;
        ptr[i].ValueCkUlong = 0;

        ptr[i].AttributeType = (uint32_t)pTemplate[i].type;
        ptr[i].ValueTypeHint = AttrValueFromNative_TypeHint_Binary;
        ptr[i].ValueRawBytes.data = (uint8_t*)pTemplate[i].pValue;
        ptr[i].ValueRawBytes.size = (size_t)pTemplate[i].ulValueLen;
        ptr[i].ValueBool = false;
        ptr[i].ValueCkUlong = 0;
        ptr[i].ValueCkDate = NULL;

        if (ptr[i].ValueRawBytes.size == sizeof(CK_BBOOL))
        {
            ptr[i].ValueTypeHint |= AttrValueFromNative_TypeHint_Bool;

            CK_BBOOL* boolValue = (CK_BBOOL*)pTemplate[i].pValue;
            ptr[i].ValueBool = *boolValue;
        }

        if (ptr[i].ValueRawBytes.size == sizeof(CK_ULONG))
        {
            ptr[i].ValueTypeHint |= AttrValueFromNative_TypeHint_CkUlong;

            CK_ULONG* ulongValue = (CK_ULONG*)pTemplate[i].pValue;
            ptr[i].ValueCkUlong = (uint32_t)(*ulongValue);
        }

        if (ptr[i].ValueRawBytes.size == 0)
        {
            ptr[i].ValueTypeHint |= AttrValueFromNative_TypeHint_CkDate;
        }

        if (ptr[i].ValueRawBytes.size == sizeof(CK_DATE))
        {
            CK_DATE* dateValue = (CK_DATE*)pTemplate[i].pValue;

            if (isdigit(dateValue->day[0])
                && isdigit(dateValue->day[1])
                && isdigit(dateValue->month[0])
                && isdigit(dateValue->month[1])
                && isdigit(dateValue->year[0])
                && isdigit(dateValue->year[1])
                && isdigit(dateValue->year[2])
                && isdigit(dateValue->year[3])
                )
            {
                ptr[i].ValueTypeHint |= AttrValueFromNative_TypeHint_CkDate;
                char* date = (char*)malloc(12);
                if (date == NULL)
                {
                    if (ptr != NULL)
                    {
                        free((void*)ptr);
                    }

                    return NULL;
                }

                date[0] = dateValue->day[0];
                date[1] = dateValue->day[1];
                date[2] = '.';
                date[3] = dateValue->month[0];
                date[4] = dateValue->month[1];
                date[5] = '.';
                date[6] = dateValue->year[0];
                date[7] = dateValue->year[1];
                date[8] = dateValue->year[2];
                date[9] = dateValue->year[3];
                date[10] = 0;

                ptr[i].ValueCkDate = date;
            }

        }
    }

    return ptr;
}

void AttrValueFromNative_Destroy(AttrValueFromNative* ptr, CK_ULONG ulCount)
{
    CK_ULONG i;
    for (i = 0; i < ulCount; i++)
    {
        void* datePtr = ptr[i].ValueCkDate;
        if (datePtr != NULL)
        {
            free(datePtr);
        }
    }

    free((void*)ptr);
}

//TODO: extra definitions
#define CKM_SHA512_T 0x00000050

int MechanismValue_Create(MechanismValue* value, CK_MECHANISM_PTR pMechanism)
{
    int result = NMRPC_OK;
    value->MechanismType = (uint32_t)pMechanism->mechanism;
    value->MechanismParamMp = NULL;

    if (pMechanism->pParameter == NULL)
    {
        return NMRPC_OK;
    }

    switch (pMechanism->mechanism)
    {
    case CKM_SHA512_T:
    case CKM_MD2_HMAC_GENERAL:
    case CKM_MD5_HMAC_GENERAL:
    case CKM_RIPEMD128_HMAC_GENERAL:
    case CKM_RIPEMD160_HMAC_GENERAL:
    case CKM_SHA_1_HMAC_GENERAL:
    case CKM_SHA224_HMAC_GENERAL:
    case CKM_SHA256_HMAC_GENERAL:
    case CKM_SHA384_HMAC_GENERAL:
    case CKM_SHA512_HMAC_GENERAL:
    case CKM_SHA512_224_HMAC_GENERAL:
    case CKM_SHA512_256_HMAC_GENERAL:
    case CKM_SHA3_224_HMAC_GENERAL:
    case CKM_SHA3_256_HMAC_GENERAL:
    case CKM_SHA3_384_HMAC_GENERAL:
    case CKM_SHA3_512_HMAC_GENERAL:
    case CKM_BLAKE2B_160_HMAC_GENERAL:
    case CKM_BLAKE2B_256_HMAC_GENERAL:
    case CKM_BLAKE2B_384_HMAC_GENERAL:
    case CKM_BLAKE2B_512_HMAC_GENERAL:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_MAC_GENERAL_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_MAC_GENERAL_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_ULONG generalParamsValue = *((CK_ULONG*)pMechanism->pParameter);
        CkP_MacGeneralParams gp;
        gp.Value = (uint32_t)generalParamsValue;

        result = nmrpc_writeAsBinary(&gp, (SerializeFnPtr_t)CkP_MacGeneralParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_RSA_PKCS_PSS:
    case CKM_SHA1_RSA_PKCS_PSS:
    case CKM_SHA224_RSA_PKCS_PSS:
    case CKM_SHA256_RSA_PKCS_PSS:
    case CKM_SHA384_RSA_PKCS_PSS:
    case CKM_SHA512_RSA_PKCS_PSS:
    case CKM_SHA3_224_RSA_PKCS_PSS:
    case CKM_SHA3_256_RSA_PKCS_PSS:
    case CKM_SHA3_384_RSA_PKCS_PSS:
    case CKM_SHA3_512_RSA_PKCS_PSS:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_RSA_PKCS_PSS_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_RSA_PKCS_PSS_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_RSA_PKCS_PSS_PARAMS_PTR pssParam = ((CK_RSA_PKCS_PSS_PARAMS_PTR)pMechanism->pParameter);
        CkP_RsaPkcsPssParams ckpPssParam;

        ckpPssParam.HashAlg = (uint32_t)pssParam->hashAlg;
        ckpPssParam.Mgf = (uint32_t)pssParam->mgf;
        ckpPssParam.SLen = (uint32_t)pssParam->sLen;

        result = nmrpc_writeAsBinary(&ckpPssParam, (SerializeFnPtr_t)CkP_RsaPkcsPssParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_CONCATENATE_DATA_AND_BASE:
    case CKM_CONCATENATE_BASE_AND_DATA:
    case CKM_XOR_BASE_AND_DATA:
    case CKM_AES_ECB_ENCRYPT_DATA:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_KEY_DERIVATION_STRING_DATA))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_KEY_DERIVATION_STRING_DATA in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_KEY_DERIVATION_STRING_DATA_PTR dsd = (CK_KEY_DERIVATION_STRING_DATA_PTR)pMechanism->pParameter;
        CkP_KeyDerivationStringData derivationStringData;
        derivationStringData.Data.data = (uint8_t*)dsd->pData;
        derivationStringData.Data.size = (size_t)dsd->ulLen;
        derivationStringData.Len = (uint32_t)dsd->ulLen;

        result = nmrpc_writeAsBinary(&derivationStringData, (SerializeFnPtr_t)CkP_KeyDerivationStringData_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_AES_CBC_ENCRYPT_DATA:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_AES_CBC_ENCRYPT_DATA_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_AES_CBC_ENCRYPT_DATA_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_AES_CBC_ENCRYPT_DATA_PARAMS_PTR cedp = (CK_AES_CBC_ENCRYPT_DATA_PARAMS_PTR)pMechanism->pParameter;
        Ckp_CkAesCbcEnryptDataParams cbcData;
        cbcData.Iv.data = (uint8_t*)cedp->iv;
        cbcData.Iv.size = sizeof(cedp->iv);
        cbcData.Data.data = (uint8_t*)cedp->pData;
        cbcData.Data.size = (size_t)cedp->length;

        result = nmrpc_writeAsBinary(&cbcData, (SerializeFnPtr_t)Ckp_CkAesCbcEnryptDataParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_CONCATENATE_BASE_AND_KEY:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_OBJECT_HANDLE))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_OBJECT_HANDLE in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_OBJECT_HANDLE_PTR handlePtr = (CK_OBJECT_HANDLE_PTR)pMechanism->pParameter;
        CkP_CkObjectHandle handleParams;
        handleParams.Handle = (uint32_t)*handlePtr;

        result = nmrpc_writeAsBinary(&handleParams, (SerializeFnPtr_t)CkP_CkObjectHandle_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_EXTRACT_KEY_FROM_KEY:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_EXTRACT_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_EXTRACT_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_EXTRACT_PARAMS extractParamsValue = *((CK_EXTRACT_PARAMS*)pMechanism->pParameter);
        CkP_ExtractParams ep;
        ep.Value = (uint32_t)extractParamsValue;

        result = nmrpc_writeAsBinary(&ep, (SerializeFnPtr_t)CkP_ExtractParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_ECDH1_DERIVE:
    case CKM_ECDH1_COFACTOR_DERIVE:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_ECDH1_DERIVE_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_ECDH1_DERIVE_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_ECDH1_DERIVE_PARAMS_PTR deriveParamsPtr = (CK_ECDH1_DERIVE_PARAMS_PTR)pMechanism->pParameter;

        Ckp_CkEcdh1DeriveParams deriveParams;
        Binary sharedData;

        deriveParams.Kdf = (uint32_t)deriveParamsPtr->kdf;
        deriveParams.SharedData = NULL;
        deriveParams.PublicData.data = (uint8_t*)deriveParamsPtr->pPublicData;
        deriveParams.PublicData.size = (size_t)deriveParamsPtr->ulPublicDataLen;

        if (deriveParamsPtr->pSharedData != NULL)
        {
            sharedData.data = (uint8_t*)deriveParamsPtr->pSharedData;
            sharedData.size = (size_t)deriveParamsPtr->ulSharedDataLen;

            deriveParams.SharedData = &sharedData;
        }

        result = nmrpc_writeAsBinary(&deriveParams, (SerializeFnPtr_t)Ckp_CkEcdh1DeriveParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_AES_CBC:
    case CKM_AES_CBC_PAD:
    case CKM_AES_CFB1:
    case CKM_AES_CFB8:
    case CKM_AES_CFB64:
    case CKM_AES_CFB128:
    case CKM_AES_OFB:
    case CKM_AES_CTR:
    case CKM_AES_CTS:
    {
        if (pMechanism->pParameter == NULL)
        {
            log_message(LOG_LEVEL_ERROR, "Excepted raw data in this mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CkP_RawDataParams rawData;
        rawData.Value.data = (uint8_t*)pMechanism->pParameter;
        rawData.Value.size = (size_t)pMechanism->ulParameterLen;

        result = nmrpc_writeAsBinary(&rawData, (SerializeFnPtr_t)CkP_RawDataParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case  CKM_AES_GCM:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_GCM_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_GCM_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_GCM_PARAMS_PTR gcmParams = (CK_GCM_PARAMS_PTR)pMechanism->pParameter;

        Ckp_CkGcmParams gcmDerivedParams;
        Binary ivData;
        Binary aadData;

        gcmDerivedParams.Iv = NULL;
        gcmDerivedParams.Aad = NULL;
        gcmDerivedParams.IvBits = (uint32_t)gcmParams->ulIvBits;
        gcmDerivedParams.TagBits = (uint32_t)gcmParams->ulTagBits;


        if (gcmParams->pIv != NULL)
        {
            ivData.data = (uint8_t*)gcmParams->pIv;
            ivData.size = (size_t)gcmParams->ulIvLen;

            gcmDerivedParams.Iv = &ivData;
        }

        if (gcmParams->pAAD != NULL)
        {
            aadData.data = (uint8_t*)gcmParams->pAAD;
            aadData.size = (size_t)gcmParams->ulAADLen;

            gcmDerivedParams.Aad = &aadData;
        }

        result = nmrpc_writeAsBinary(&gcmDerivedParams, (SerializeFnPtr_t)Ckp_CkGcmParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case  CKM_AES_CCM:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_CCM_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_CCM_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_CCM_PARAMS_PTR gcmParams = (CK_CCM_PARAMS_PTR)pMechanism->pParameter;

        Ckp_CkCcmParams ccmDerivedParams;
        Binary nonceData;
        Binary aadData;

        ccmDerivedParams.DataLen = (uint32_t)gcmParams->ulDataLen;
        ccmDerivedParams.Nonce = NULL;
        ccmDerivedParams.Aad = NULL;
        ccmDerivedParams.MacLen = (uint32_t)gcmParams->ulMACLen;

        if (gcmParams->pNonce != NULL)
        {
            nonceData.data = (uint8_t*)gcmParams->pNonce;
            nonceData.size = (size_t)gcmParams->ulNonceLen;

            ccmDerivedParams.Nonce = &nonceData;
        }

        if (gcmParams->pAAD != NULL)
        {
            aadData.data = (uint8_t*)gcmParams->pAAD;
            aadData.size = (size_t)gcmParams->ulAADLen;

            ccmDerivedParams.Aad = &aadData;
        }

        result = nmrpc_writeAsBinary(&ccmDerivedParams, (SerializeFnPtr_t)Ckp_CkCcmParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case  CKM_RSA_PKCS_OAEP:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_RSA_PKCS_OAEP_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_RSA_PKCS_OAEP_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_RSA_PKCS_OAEP_PARAMS_PTR oaepParams = (CK_RSA_PKCS_OAEP_PARAMS_PTR)pMechanism->pParameter;

        Ckp_CkRsaPkcsOaepParams oaepDervedParams;
        Binary sourceData;

        oaepDervedParams.HashAlg = (uint32_t)oaepParams->hashAlg;
        oaepDervedParams.Mgf = (uint32_t)oaepParams->mgf;
        oaepDervedParams.Source = (uint32_t)oaepParams->source;
        oaepDervedParams.SourceData = NULL;

        if (oaepParams->pSourceData != NULL)
        {
            sourceData.data = (uint8_t*)oaepParams->pSourceData;
            sourceData.size = (size_t)oaepParams->ulSourceDataLen;

            oaepDervedParams.SourceData = &sourceData;
        }

        result = nmrpc_writeAsBinary(&oaepDervedParams, (SerializeFnPtr_t)Ckp_CkRsaPkcsOaepParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_CHACHA20:
        if (pMechanism->ulParameterLen != sizeof(CK_CHACHA20_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_CHACHA20_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_CHACHA20_PARAMS_PTR chaCha20Params = (CK_CHACHA20_PARAMS_PTR)pMechanism->pParameter;
        Ckp_CkChaCha20Params chaCha20DeriveParams = { 0 };

        if (chaCha20Params->pNonce == NULL && chaCha20Params->ulNonceBits != 0)
        {
            log_message(LOG_LEVEL_ERROR, "Nonce value in CK_CHACHA20_PARAMS_PTR is NULL and ulNonceBits is not zero.");
            return NMRPC_FATAL_ERROR;
        }

        chaCha20DeriveParams.BlockCounterUpper = 0;
        chaCha20DeriveParams.BlockCounterLower = 0;
        chaCha20DeriveParams.BlockCounterBits = (uint32_t)chaCha20Params->blockCounterBits;
        chaCha20DeriveParams.BlockCounterIsSet = false;
        chaCha20DeriveParams.Nonce.data = (uint8_t*)chaCha20Params->pNonce;
        chaCha20DeriveParams.Nonce.size = (size_t)(chaCha20Params->ulNonceBits / 8);

        if (chaCha20Params->pBlockCounter == NULL)
        {
            log_message(LOG_LEVEL_TRACE, "pBlockCounter value in CK_CHACHA20_PARAMS_PTR is NULL");
            chaCha20DeriveParams.BlockCounterIsSet = false;
        }
        else
        {
            chaCha20DeriveParams.BlockCounterIsSet = true;
            if (chaCha20Params->blockCounterBits == 32)
            {
                uint32_t blockCounterValue = *((uint32_t*)chaCha20Params->pBlockCounter);
                chaCha20DeriveParams.BlockCounterUpper = 0;
                chaCha20DeriveParams.BlockCounterLower = blockCounterValue;
            }

            if (chaCha20Params->blockCounterBits == 64)
            {
                uint64_t blockCounterValue = *((uint64_t*)chaCha20Params->pBlockCounter);
                chaCha20DeriveParams.BlockCounterUpper = (uint32_t)((blockCounterValue >> 32) & 0xFFFFFFFF);
                chaCha20DeriveParams.BlockCounterLower = (uint32_t)(blockCounterValue & 0xFFFFFFFF);
            }
        }

        result = nmrpc_writeAsBinary(&chaCha20DeriveParams, (SerializeFnPtr_t)Ckp_CkChaCha20Params_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
        break;

    case CKM_SALSA20:
        if (pMechanism->ulParameterLen != sizeof(CK_SALSA20_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_SALSA20_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_SALSA20_PARAMS_PTR salsa20Params = (CK_SALSA20_PARAMS_PTR)pMechanism->pParameter;
        Ckp_CkSalsa20Params salsa20DeriveParams = { 0 };

        if (salsa20Params->pNonce == NULL)
        {
            log_message(LOG_LEVEL_ERROR, "Nonce value in CK_CK_SALSA20_PARAMS_PTR is NULL and ulNonceBits is not zero.");
            return NMRPC_FATAL_ERROR;
        }

        salsa20DeriveParams.BlockCounter = 0;
        salsa20DeriveParams.BlockCounterIsSet = false;
        salsa20DeriveParams.Nonce.data = (uint8_t*)salsa20Params->pNonce;
        salsa20DeriveParams.Nonce.size = (size_t)(salsa20Params->ulNonceBits / 8);

        if (salsa20Params->pBlockCounter == NULL)
        {
            log_message(LOG_LEVEL_TRACE, "pBlockCounter value in CK_CK_SALSA20_PARAMS_PTR is NULL");
            salsa20DeriveParams.BlockCounterIsSet = false;
        }
        else
        {
            salsa20DeriveParams.BlockCounterIsSet = true;
            salsa20DeriveParams.BlockCounter= *((uint64_t*)salsa20Params->pBlockCounter);
        }

        result = nmrpc_writeAsBinary(&salsa20DeriveParams, (SerializeFnPtr_t)Ckp_CkSalsa20Params_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
        break;

    case CKM_CHACHA20_POLY1305:
    {
        //CK_SALSA20_CHACHA20_POLY1305_PARAMS
        if (pMechanism->ulParameterLen != sizeof(CK_SALSA20_CHACHA20_POLY1305_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_SALSA20_CHACHA20_POLY1305_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_SALSA20_CHACHA20_POLY1305_PARAMS_PTR salsa20Chacha20Poly1305Params = (CK_SALSA20_CHACHA20_POLY1305_PARAMS_PTR)pMechanism->pParameter;
        Ckp_CkSalsa20ChaCha20Poly1305Params salsa20Chacha20Poly1305Derivedparams = { 0 };

        if (salsa20Chacha20Poly1305Params->pNonce == NULL || salsa20Chacha20Poly1305Params->ulNonceLen == 0)
        {
            log_message(LOG_LEVEL_ERROR, "Nonce value in CK_SALSA20_CHACHA20_POLY1305_PARAMS_PTR is NULL and ulNonceLen is not zero.");
            return NMRPC_FATAL_ERROR;
        }

        Binary aadData;

        salsa20Chacha20Poly1305Derivedparams.Nonce.data = (uint8_t*)salsa20Chacha20Poly1305Params->pNonce;
        salsa20Chacha20Poly1305Derivedparams.Nonce.size = (size_t)salsa20Chacha20Poly1305Params->ulNonceLen;
        salsa20Chacha20Poly1305Derivedparams.AadData = NULL;
        if (salsa20Chacha20Poly1305Params->pAAD != NULL)
        {
            aadData.data = (uint8_t*)salsa20Chacha20Poly1305Params->pAAD;
            aadData.size = (size_t)salsa20Chacha20Poly1305Params->ulAADLen;
            salsa20Chacha20Poly1305Derivedparams.AadData = &aadData;
        }

        result = nmrpc_writeAsBinary(&salsa20Chacha20Poly1305Derivedparams, (SerializeFnPtr_t)Ckp_CkSalsa20ChaCha20Poly1305Params_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    case CKM_EDDSA:
    {
        if (pMechanism->ulParameterLen != sizeof(CK_EDDSA_PARAMS))
        {
            log_message(LOG_LEVEL_ERROR, "Excepted CK_EDDSA_PARAMS in mechanism.");
            return NMRPC_FATAL_ERROR;
        }

        CK_EDDSA_PARAMS_PTR eddsaParams = (CK_EDDSA_PARAMS_PTR)pMechanism->pParameter;
        Ckp_CkEddsaParams eddsaParamsDerivedparams = { 0 };
        Binary contextData;

        eddsaParamsDerivedparams.PhFlag = (bool) eddsaParams->phFlag;
        eddsaParamsDerivedparams.ContextData = NULL;
        if (eddsaParams->pContextData != NULL)
        {
            contextData.data = (uint8_t*)eddsaParams->pContextData;
            contextData.size = (size_t)eddsaParams->ulContextDataLen;
            eddsaParamsDerivedparams.ContextData = &contextData;
        }

        result = nmrpc_writeAsBinary(&eddsaParamsDerivedparams, (SerializeFnPtr_t)Ckp_CkEddsaParams_Serialize, &value->MechanismParamMp);
        if (result != NMRPC_OK)
        {
            return result;
        }
    }
    break;

    default:
        break;
    }

    return NMRPC_OK;
}

void MechanismValue_Destroy(MechanismValue* value)
{
    if (value->MechanismParamMp != NULL)
    {
        Binary_Release(value->MechanismParamMp);
        free((void*)value->MechanismParamMp);
        value->MechanismParamMp = NULL;
    }
}

void InitCallContext(nmrpc_global_context_t* ctxPtr, AppIdentification* appId)
{
    *appId = globalContext.appId;

    if (globalContext.tag[0] != 0)
    {
        ctxPtr->tag = globalContext.tag;
    }
    else
    {
        ctxPtr->tag = NULL;
    }

#ifndef _WIN32
    appId->Pid = (uint64_t)getpid();
#endif
}


CK_FUNCTION_LIST empty_pkcs11_functions =
{
    {2, 40},
    &C_Initialize,
    &C_Finalize,
    &C_GetInfo,
    &C_GetFunctionList,
    &C_GetSlotList,
    &C_GetSlotInfo,
    &C_GetTokenInfo,
    &C_GetMechanismList,
    &C_GetMechanismInfo,
    &C_InitToken,
    &C_InitPIN,
    &C_SetPIN,
    &C_OpenSession,
    &C_CloseSession,
    &C_CloseAllSessions,
    &C_GetSessionInfo,
    &C_GetOperationState,
    &C_SetOperationState,
    &C_Login,
    &C_Logout,
    &C_CreateObject,
    &C_CopyObject,
    &C_DestroyObject,
    &C_GetObjectSize,
    &C_GetAttributeValue,
    &C_SetAttributeValue,
    &C_FindObjectsInit,
    &C_FindObjects,
    &C_FindObjectsFinal,
    &C_EncryptInit,
    &C_Encrypt,
    &C_EncryptUpdate,
    &C_EncryptFinal,
    &C_DecryptInit,
    &C_Decrypt,
    &C_DecryptUpdate,
    &C_DecryptFinal,
    &C_DigestInit,
    &C_Digest,
    &C_DigestUpdate,
    &C_DigestKey,
    &C_DigestFinal,
    &C_SignInit,
    &C_Sign,
    &C_SignUpdate,
    &C_SignFinal,
    &C_SignRecoverInit,
    &C_SignRecover,
    &C_VerifyInit,
    &C_Verify,
    &C_VerifyUpdate,
    &C_VerifyFinal,
    &C_VerifyRecoverInit,
    &C_VerifyRecover,
    &C_DigestEncryptUpdate,
    &C_DecryptDigestUpdate,
    &C_SignEncryptUpdate,
    &C_DecryptVerifyUpdate,
    &C_GenerateKey,
    &C_GenerateKeyPair,
    &C_WrapKey,
    &C_UnwrapKey,
    &C_DeriveKey,
    &C_SeedRandom,
    &C_GenerateRandom,
    &C_GetFunctionStatus,
    &C_CancelFunction,
    &C_WaitForSlotEvent
};

#define P11SocketInit(tcpPtr) SockContext_init((tcpPtr), globalContext.server, globalContext.port)
#define ValueHasFlag(value, flag) (((value) & (flag)) == (flag))


//PeriodicTimer_t pingTimer = { 0 };

void ExecutePing(void* pUserData)
{
    PingRequest request;
    PingEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        log_message(LOG_LEVEL_ERROR, "P11SocketInit failed in line %i in function %s.", __LINE__, __FUNCTION__);
        return;
    }

    nmrpc_global_context_tcp_init(&ctx, &tcp);

    request.AppId = globalContext.appId;

    int rv = nmrpc_call_Ping(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        return;
    }

    PingEnvelope_Release(&envelope);
}

CK_DEFINE_FUNCTION(CK_RV, C_Initialize)(CK_VOID_PTR pInitArgs)
{
    LOG_ENTERING_TO_FUNCTION();

    InitializeRequest request;
    InitializeEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }

    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    if (NULL == pInitArgs)
    {
        request.IsMutexFnSet = false;
        request.LibraryCantCreateOsThreads = false;
        request.OsLockingOk = false;
    }
    else
    {
        CK_C_INITIALIZE_ARGS* pInitArgsTyped = (CK_C_INITIALIZE_ARGS*)pInitArgs;
        request.IsMutexFnSet = (pInitArgsTyped->DestroyMutex) != NULL;
        request.LibraryCantCreateOsThreads = ValueHasFlag(pInitArgsTyped->flags, CKF_LIBRARY_CANT_CREATE_OS_THREADS);
        request.OsLockingOk = ValueHasFlag(pInitArgsTyped->flags, CKF_OS_LOCKING_OK);
    }

    request.ClientInfo.CkUlongSize = sizeof(CK_ULONG);
    request.ClientInfo.PointerSize = sizeof(void*);
    request.ClientInfo.LibVersion = BOUNCY_HSM_LIBVERSION;
    request.ClientInfo.Platform = "Unknown";
    request.ClientInfo.CompiuterName = "";

    char compiuterName[256];
    if (GetCurrentCompiuterName(compiuterName, sizeof(compiuterName)))
    {
        request.ClientInfo.CompiuterName = compiuterName;
    }

    request.ClientInfo.Platform = GetPlatformName();

    int rv = nmrpc_call_Initialize(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    InitializeEnvelope_Release(&envelope);

    //TODO: fix crash application
    /*if (!PeriodicTimer_Create(&pingTimer, ExecutePing, NULL, 5000))
    {
        return CKR_GENERAL_ERROR;
    }*/

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Finalize)(CK_VOID_PTR pReserved)
{
    LOG_ENTERING_TO_FUNCTION();

    FinalizeRequest request;
    FinalizeEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.IsPtrSet = pReserved != NULL;

    int rv = nmrpc_call_Finalize(&ctx, &request, &envelope);

    //TODO
    //PeriodicTimer_Destroy(&pingTimer);

    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    FinalizeEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetInfo)(CK_INFO_PTR pInfo)
{
    LOG_ENTERING_TO_FUNCTION();

    GetInfoRequest request;
    GetInfoEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);


    int rv = nmrpc_call_GetInfo(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        pInfo->cryptokiVersion.major = 2;
        pInfo->cryptokiVersion.minor = 40;

        pInfo->flags = 0;

        SetPaddedStrSafe(pInfo->libraryDescription, sizeof(pInfo->libraryDescription), PKCS11_LIB_DESCRIPTION);

        pInfo->libraryVersion.major = BOUNCY_HSM_LIBVERSION_MAJOR;
        pInfo->libraryVersion.minor = BOUNCY_HSM_LIBVERSION_MINOR;

        memset(pInfo->manufacturerID, ' ', sizeof(pInfo->manufacturerID));
        memcpy(pInfo->manufacturerID, envelope.ManufacturerID, strlen(envelope.ManufacturerID));
    }

    GetInfoEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetFunctionList)(CK_FUNCTION_LIST_PTR_PTR ppFunctionList)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == ppFunctionList)
    {
        return CKR_ARGUMENTS_BAD;
    }

    *ppFunctionList = &empty_pkcs11_functions;

    return CKR_OK;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetSlotList)(CK_BBOOL tokenPresent, CK_SLOT_ID_PTR pSlotList, CK_ULONG_PTR pulCount)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pulCount)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetSlotListRequest request;
    GetSlotListEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }

    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.IsTokenPresent = (bool)tokenPresent;
    request.IsSlotListPointerPresent = pSlotList != NULL;
    request.PullCount = (uint32_t)*pulCount;

    int rv = nmrpc_call_GetSlotList(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *pulCount = (CK_ULONG)envelope.PullCount;

        if (pSlotList != NULL)
        {
            uint32_t i;
            for (i = 0; i < envelope.PullCount; i++)
            {
                pSlotList[i] = (CK_SLOT_ID)envelope.Slots.array[i];
            }
        }
    }

    GetSlotListEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetSlotInfo)(CK_SLOT_ID slotID, CK_SLOT_INFO_PTR pInfo)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pInfo)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetSlotInfoRequest request;
    GetSlotInfoEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SlotId = (uint64_t)slotID;

    int rv = nmrpc_call_GetSlotInfo(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        CK_FLAGS flags = 0;

        if (envelope.Data->FlagsTokenPresent) flags |= CKF_TOKEN_PRESENT;
        if (envelope.Data->FlagsRemovableDevice) flags |= CKF_REMOVABLE_DEVICE;
        if (envelope.Data->FlagsHwSlot) flags |= CKF_HW_SLOT;

        SetPaddedStrSafe(pInfo->slotDescription, sizeof(pInfo->slotDescription), envelope.Data->SlotDescription);
        SetPaddedStrSafe(pInfo->manufacturerID, sizeof(pInfo->manufacturerID), envelope.Data->ManufacturerID);

        pInfo->flags = flags;
        pInfo->hardwareVersion.major = (CK_BYTE)envelope.Data->HardwareVersion.Major;
        pInfo->hardwareVersion.minor = (CK_BYTE)envelope.Data->HardwareVersion.Minor;
        pInfo->firmwareVersion.major = (CK_BYTE)envelope.Data->FirmwareVersion.Major;
        pInfo->firmwareVersion.minor = (CK_BYTE)envelope.Data->FirmwareVersion.Minor;
    }

    GetSlotInfoEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetTokenInfo)(CK_SLOT_ID slotID, CK_TOKEN_INFO_PTR pInfo)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pInfo)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetTokenInfoRequest request;
    GetTokenInfoEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SlotId = (uint64_t)slotID;

    int rv = nmrpc_call_GetTokenInfo(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        SetPaddedStrSafe(pInfo->label, sizeof(pInfo->label), envelope.Data->Label);
        SetPaddedStrSafe(pInfo->manufacturerID, sizeof(pInfo->manufacturerID), envelope.Data->ManufacturerId);
        SetPaddedStrSafe(pInfo->model, sizeof(pInfo->model), envelope.Data->Model);
        SetPaddedStrSafe(pInfo->serialNumber, sizeof(pInfo->serialNumber), envelope.Data->SerialNumber);
        pInfo->flags = (CK_FLAGS)envelope.Data->Flags;
        pInfo->ulMaxSessionCount = ConvertCkSpecialUint(envelope.Data->MaxSessionCount);
        pInfo->ulRwSessionCount = ConvertCkSpecialUint(envelope.Data->RwSessionCount);
        pInfo->ulMaxPinLen = (CK_ULONG)envelope.Data->MaxPinLen;
        pInfo->ulMinPinLen = (CK_ULONG)envelope.Data->MaxPinLen;
        pInfo->ulTotalPublicMemory = ConvertCkSpecialUint(envelope.Data->TotalPublicMemory);
        pInfo->ulFreePublicMemory = ConvertCkSpecialUint(envelope.Data->FreePublicMemory);
        pInfo->ulTotalPrivateMemory = ConvertCkSpecialUint(envelope.Data->TotalPrivateMemory);
        pInfo->ulFreePrivateMemory = ConvertCkSpecialUint(envelope.Data->FreePrivateMemory);
        pInfo->hardwareVersion.major = (CK_BYTE)envelope.Data->HardwareVersion.Major;
        pInfo->hardwareVersion.minor = (CK_BYTE)envelope.Data->HardwareVersion.Minor;
        pInfo->firmwareVersion.major = (CK_BYTE)envelope.Data->FirmwareVersion.Major;
        pInfo->firmwareVersion.minor = (CK_BYTE)envelope.Data->FirmwareVersion.Minor;
        SetPaddedStrSafe(pInfo->utcTime, sizeof(pInfo->utcTime), envelope.Data->UtcTime);
    }

    GetTokenInfoEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetMechanismList)(CK_SLOT_ID slotID, CK_MECHANISM_TYPE_PTR pMechanismList, CK_ULONG_PTR pulCount)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pulCount)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetMechanismListRequest request;
    GetMechanismListEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SlotId = (uint32_t)slotID;
    request.IsMechanismListPointerPresent = pMechanismList != NULL;
    request.PullCount = (uint32_t)*pulCount;

    int rv = nmrpc_call_GetMechanismList(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *pulCount = (CK_ULONG)envelope.Data->PullCount;

        if (pMechanismList != NULL)
        {
            uint32_t i;
            for (i = 0; i < envelope.Data->PullCount; i++)
            {
                pMechanismList[i] = (CK_MECHANISM_TYPE)envelope.Data->MechanismTypes.array[i];
            }
        }
    }

    GetMechanismListEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetMechanismInfo)(CK_SLOT_ID slotID, CK_MECHANISM_TYPE type, CK_MECHANISM_INFO_PTR pInfo)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pInfo)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetMechanismInfoRequest request;
    GetMechanismInfoEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SlotId = (uint32_t)slotID;
    request.MechanismType = type;

    int rv = nmrpc_call_GetMechanismInfo(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        pInfo->ulMinKeySize = envelope.Data->MinKeySize;
        pInfo->ulMaxKeySize = envelope.Data->MaxKeySize;
        pInfo->flags = (CK_FLAGS)envelope.Data->Flags;
    }

    GetMechanismInfoEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_InitToken)(CK_SLOT_ID slotID, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen, CK_UTF8CHAR_PTR pLabel)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_InitPIN)(CK_SESSION_HANDLE hSession, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_SetPIN)(CK_SESSION_HANDLE hSession, CK_UTF8CHAR_PTR pOldPin, CK_ULONG ulOldLen, CK_UTF8CHAR_PTR pNewPin, CK_ULONG ulNewLen)
{
    LOG_ENTERING_TO_FUNCTION();

    SetPinRequest request;
    SetPinEnvelope envelope;
    Binary oldPinBinary;
    Binary newPinBinary;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Utf8OldPin = NULL;
    request.Utf8NewPin = NULL;

    if (NULL != pOldPin)
    {
        oldPinBinary.data = (uint8_t*)pOldPin;
        oldPinBinary.size = (size_t)ulOldLen;

        request.Utf8OldPin = &oldPinBinary;
    }

    if (NULL != pOldPin)
    {
        newPinBinary.data = (uint8_t*)pNewPin;
        newPinBinary.size = (size_t)ulNewLen;

        request.Utf8NewPin = &newPinBinary;
    }

    int rv = nmrpc_call_SetPin(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    SetPinEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_OpenSession)(CK_SLOT_ID slotID, CK_FLAGS flags, CK_VOID_PTR pApplication, CK_NOTIFY Notify, CK_SESSION_HANDLE_PTR phSession)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == phSession)
    {
        return CKR_ARGUMENTS_BAD;
    }

    OpenSessionRequest request;
    OpenSessionEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SlotId = (uint32_t)slotID;
    request.Flags = (uint32_t)flags;
    request.IsPtrApplicationSet = pApplication != NULL;
    request.IsNotifySet = Notify != NULL;

    int rv = nmrpc_call_OpenSession(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phSession = (CK_SESSION_HANDLE)envelope.SessionId;
    }

    OpenSessionEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_CloseSession)(CK_SESSION_HANDLE hSession)
{
    LOG_ENTERING_TO_FUNCTION();

    CloseSessionRequest request;
    CloseSessionEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;

    int rv = nmrpc_call_CloseSession(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    CloseSessionEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_CloseAllSessions)(CK_SLOT_ID slotID)
{
    LOG_ENTERING_TO_FUNCTION();

    CloseAllSessionsRequest request;
    CloseAllSessionsEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SlotId = (uint32_t)slotID;

    int rv = nmrpc_call_CloseAllSessions(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    CloseAllSessionsEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetSessionInfo)(CK_SESSION_HANDLE hSession, CK_SESSION_INFO_PTR pInfo)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pInfo)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetSessionInfoRequest request;
    GetSessionInfoEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;

    int rv = nmrpc_call_GetSessionInfo(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        pInfo->slotID = envelope.Data->SlotId;
        pInfo->state = envelope.Data->State;
        pInfo->flags = (CK_FLAGS)envelope.Data->Flags;
        pInfo->ulDeviceError = envelope.Data->DeviceError;
    }

    GetSessionInfoEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetOperationState)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pOperationState, CK_ULONG_PTR pulOperationStateLen)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_SetOperationState)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pOperationState, CK_ULONG ulOperationStateLen, CK_OBJECT_HANDLE hEncryptionKey, CK_OBJECT_HANDLE hAuthenticationKey)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_Login)(CK_SESSION_HANDLE hSession, CK_USER_TYPE userType, CK_UTF8CHAR_PTR pPin, CK_ULONG ulPinLen)
{
    LOG_ENTERING_TO_FUNCTION();

    LoginRequest request;
    LoginEnvelope envelope;
    Binary pinBinary;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.UserType = (uint32_t)userType;
    request.Utf8Pin = NULL;

    if (NULL != pPin)
    {
        pinBinary.data = (uint8_t*)pPin;
        pinBinary.size = (size_t)ulPinLen;

        request.Utf8Pin = &pinBinary;
    }

    int rv = nmrpc_call_Login(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    LoginEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Logout)(CK_SESSION_HANDLE hSession)
{
    LOG_ENTERING_TO_FUNCTION();

    LogoutRequest request;
    LogoutEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;

    int rv = nmrpc_call_Logout(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    LogoutEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_CreateObject)(CK_SESSION_HANDLE hSession, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phObject)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pTemplate || 0 == ulCount || NULL == phObject)
    {
        return CKR_ARGUMENTS_BAD;
    }

    CreateObjectRequest request;
    CreateObjectEnvelope envelope;
    AttrValueFromNative* attrTemplate = NULL;

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulCount);
    if (NULL == attrTemplate)
    {
        return CKR_GENERAL_ERROR;
    }

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Template.array = attrTemplate;
    request.Template.length = (int)ulCount;

    int rv = nmrpc_call_CreateObject(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulCount);
        }

        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phObject = (CK_ULONG)envelope.ObjectHandle;
    }

    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulCount);
    }

    CreateObjectEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_CopyObject)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phNewObject)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pTemplate && ulCount > 0)
    {
        return CKR_ARGUMENTS_BAD;
    }

    if (NULL == phNewObject)
    {
        return CKR_ARGUMENTS_BAD;
    }

    CopyObjectRequest request;
    CopyObjectEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    AttrValueFromNative* attrTemplate = NULL;

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulCount);
    if (NULL == attrTemplate)
    {
        return CKR_GENERAL_ERROR;
    }

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.ObjectHandle = (uint32_t)hObject;
    request.Template.array = attrTemplate;
    request.Template.length = (int)ulCount;

    int rv = nmrpc_call_CopyObject(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulCount);
        }
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phNewObject = (CK_OBJECT_HANDLE)envelope.Data->ObjectHandle;
    }

    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulCount);
    }

    CopyObjectEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DestroyObject)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE hObject)
{
    LOG_ENTERING_TO_FUNCTION();

    DestroyObjectRequest request;
    DestroyObjectEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.ObjectHandle = (uint32_t)hObject;

    int rv = nmrpc_call_DestroyObject(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    DestroyObjectEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetObjectSize)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE hObject, CK_ULONG_PTR pulSize)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pulSize)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetObjectSizeRequest request;
    GetObjectSizeEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.ObjectHandle = (uint32_t)hObject;

    int rv = nmrpc_call_GetObjectSize(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *pulSize = (CK_ULONG)ConvertCkSpecialUint(*envelope.Data);
    }

    GetObjectSizeEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetAttributeValue)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pTemplate)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GetAttributeValueRequest request;
    GetAttributeValueEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.ObjectHandle = (uint32_t)hObject;

    request.InTemplate.array = (GetAttributeInputValues*)malloc(sizeof(GetAttributeInputValues) * ulCount);
    request.InTemplate.length = (int)ulCount;

    CK_ULONG i;
    for (i = 0; i < ulCount; i++)
    {
        GetAttributeInputValues* attrPtr = &request.InTemplate.array[i];

        attrPtr->AttributeType = (uint32_t)pTemplate[i].type;
        attrPtr->IsValuePtrSet = pTemplate[i].pValue != NULL;
        attrPtr->ValueLen = (uint32_t)pTemplate[i].ulValueLen;
    }

    int rv = nmrpc_call_GetAttributeValue(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();

        if (request.InTemplate.array != NULL)
        {
            free((void*)request.InTemplate.array);
        }

        return CKR_DEVICE_ERROR;
    }

    CK_RV rvMethod = (CK_RV)envelope.Rv;

    if (rv == CKR_OK || rv == CKR_ATTRIBUTE_SENSITIVE || rv == CKR_ATTRIBUTE_TYPE_INVALID || rv == CKR_BUFFER_TOO_SMALL)
    {
        for (i = 0; i < ulCount; i++)
        {
            GetAttributeOutValue* outAttrPtr = &envelope.Data->OutTemplate.array[i];
            CK_ULONG newValueLen = 0;

            if (!outAttrPtr->ValueLen.EffectivelyInfinite
                && !outAttrPtr->ValueLen.InformationSensitive
                && !outAttrPtr->ValueLen.UnavailableInformation)
            {
                if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_Binary)
                {
                    newValueLen = ConvertCkSpecialUint(outAttrPtr->ValueLen);
                }
                else if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_Bool)
                {
                    newValueLen = sizeof(CK_BBOOL);
                }
                else if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_CkUlong)
                {
                    newValueLen = sizeof(CK_ULONG);
                }
                else if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_CkDate)
                {
                    newValueLen = (outAttrPtr->ValueCkDate == NULL) ? 0 : sizeof(CK_DATE);
                }
                else
                {
                    log_message(LOG_LEVEL_ERROR, "Invalid ValueType %i on line %i in function %s.", (int)outAttrPtr->ValueType, __LINE__, __FUNCTION__);
                    return CKR_DEVICE_ERROR;
                }
            }
            else
            {
                newValueLen = ConvertCkSpecialUint(outAttrPtr->ValueLen);
            }

            if (pTemplate[i].pValue != NULL)
            {
                if (newValueLen < pTemplate[i].ulValueLen)
                {
                    rvMethod = CKR_BUFFER_TOO_SMALL;
                    pTemplate[i].ulValueLen = newValueLen;
                    continue;
                }

                if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_Binary)
                {
                    memcpy(pTemplate[i].pValue, outAttrPtr->ValueBytes.data, outAttrPtr->ValueBytes.size);
                }
                else if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_Bool)
                {
                    *((CK_BBOOL*)pTemplate[i].pValue) = (CK_BBOOL)(outAttrPtr->ValueBool ? CK_TRUE : CK_FALSE);
                }
                else if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_CkUlong)
                {
                    *((CK_ULONG*)pTemplate[i].pValue) = (CK_ULONG)outAttrPtr->ValueUint;
                }
                else if (outAttrPtr->ValueType == AttrValueToNative_TypeHint_CkDate)
                {
                    if (outAttrPtr->ValueCkDate != NULL)
                    {
                        CK_DATE* date = (CK_DATE*)pTemplate[i].pValue;

                        date->day[0] = (CK_CHAR)outAttrPtr->ValueCkDate[0];
                        date->day[1] = (CK_CHAR)outAttrPtr->ValueCkDate[1];
                        date->month[0] = (CK_CHAR)outAttrPtr->ValueCkDate[3];
                        date->month[1] = (CK_CHAR)outAttrPtr->ValueCkDate[4];
                        date->year[0] = (CK_CHAR)outAttrPtr->ValueCkDate[6];
                        date->year[1] = (CK_CHAR)outAttrPtr->ValueCkDate[7];
                        date->year[2] = (CK_CHAR)outAttrPtr->ValueCkDate[8];
                        date->year[3] = (CK_CHAR)outAttrPtr->ValueCkDate[9];
                    }
                }
                else
                {
                    log_message(LOG_LEVEL_ERROR, "Invalid ValueType %i on line %i in function %s.", (int)outAttrPtr->ValueType, __LINE__, __FUNCTION__);
                    return CKR_DEVICE_ERROR;
                }
            }

            pTemplate[i].ulValueLen = newValueLen;
        }
    }

    if (request.InTemplate.array != NULL)
    {
        free((void*)request.InTemplate.array);
    }

    GetAttributeValueEnvelope_Release(&envelope);

    return rvMethod;
}


CK_DEFINE_FUNCTION(CK_RV, C_SetAttributeValue)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE hObject, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pTemplate || ulCount == 0)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SetAttributeValueRequest request;
    SetAttributeValueEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    AttrValueFromNative* attrTemplate = NULL;

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulCount);
    if (NULL == attrTemplate)
    {
        return CKR_GENERAL_ERROR;
    }

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.ObjectHandle = (uint32_t)hObject;
    request.Template.array = attrTemplate;
    request.Template.length = (int)ulCount;

    int rv = nmrpc_call_SetAttributeValue(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulCount);
        }
        return CKR_DEVICE_ERROR;
    }

    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulCount);
    }

    SetAttributeValueEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}

CK_DEFINE_FUNCTION(CK_RV, C_FindObjectsInit)(CK_SESSION_HANDLE hSession, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pTemplate && ulCount != 0)
    {
        return CKR_ARGUMENTS_BAD;
    }

    FindObjectsInitRequest request;
    FindObjectsInitEnvelope envelope;
    AttrValueFromNative* attrTemplate = NULL;

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulCount);
    if (NULL == attrTemplate)
    {
        return CKR_GENERAL_ERROR;
    }

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Template.array = attrTemplate;
    request.Template.length = (int)ulCount;

    int rv = nmrpc_call_FindObjectsInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulCount);
        }

        return CKR_DEVICE_ERROR;
    }

    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulCount);
    }

    FindObjectsInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_FindObjects)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE_PTR phObject, CK_ULONG ulMaxObjectCount, CK_ULONG_PTR pulObjectCount)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == phObject || NULL == pulObjectCount)
    {
        return CKR_ARGUMENTS_BAD;
    }

    FindObjectsRequest request;
    FindObjectsEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.MaxObjectCount = (uint32_t)ulMaxObjectCount;

    int rv = nmrpc_call_FindObjects(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *pulObjectCount = (CK_ULONG)envelope.Data->PullObjectCount;
        size_t i;

        for (i = 0; i < (size_t)envelope.Data->Objects.length; i++)
        {
            phObject[i] = (CK_ULONG)envelope.Data->Objects.array[i];
        }
    }

    FindObjectsEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_FindObjectsFinal)(CK_SESSION_HANDLE hSession)
{
    LOG_ENTERING_TO_FUNCTION();

    FindObjectsFinalRequest request;
    FindObjectsFinalEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;

    int rv = nmrpc_call_FindObjectsFinal(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    FindObjectsFinalEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_EncryptInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism)
    {
        return CKR_ARGUMENTS_BAD;
    }

    EncryptInitRequest request;
    EncryptInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_GENERAL_ERROR;
    }

    request.KeyObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_EncryptInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    EncryptInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Encrypt)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pEncryptedData, CK_ULONG_PTR pulEncryptedDataLen)
{
    LOG_ENTERING_TO_FUNCTION();
    if (NULL == pData || NULL == pulEncryptedDataLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    EncryptRequest request;
    EncryptEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pData;
    request.Data.size = (size_t)ulDataLen;
    request.IsEncryptedDataPtrSet = pEncryptedData != NULL;
    request.EncryptedDataLen = (uint32_t)*pulEncryptedDataLen;

    int rv = nmrpc_call_Encrypt(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pEncryptedData != NULL)
        {
            memcpy_s(pEncryptedData, *pulEncryptedDataLen, envelope.Data->EncryptedData.data, envelope.Data->EncryptedData.size);
            *pulEncryptedDataLen = (CK_ULONG)envelope.Data->EncryptedData.size;
        }
        else
        {
            *pulEncryptedDataLen = (CK_ULONG)envelope.Data->PullEncryptedDataLen;
        }
    }

    EncryptEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_EncryptUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pPart)
    {
        return CKR_ARGUMENTS_BAD;
    }

    EncryptUpdateRequest request;
    EncryptUpdateEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.PartData.data = (uint8_t*)pPart;
    request.PartData.size = (size_t)ulPartLen;
    request.EncryptedDataLen = (size_t)*pulEncryptedPartLen;
    request.IsEncryptedDataPtrSet = pEncryptedPart != NULL;

    int rv = nmrpc_call_EncryptUpdate(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pEncryptedPart != NULL)
        {
            memcpy_s(pEncryptedPart, *pulEncryptedPartLen, envelope.Data->EncryptedData.data, envelope.Data->EncryptedData.size);
            *pulEncryptedPartLen = (CK_ULONG)envelope.Data->EncryptedData.size;
        }
        else
        {
            *pulEncryptedPartLen = (CK_ULONG)envelope.Data->PullEncryptedDataLen;
        }
    }

    EncryptUpdateEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_EncryptFinal)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pLastEncryptedPart, CK_ULONG_PTR pulLastEncryptedPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    EncryptFinalRequest request;
    EncryptFinalEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.IsEncryptedDataPtrSet = pLastEncryptedPart != NULL;
    request.EncryptedDataLen = (uint32_t)*pulLastEncryptedPartLen;

    int rv = nmrpc_call_EncryptFinal(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pLastEncryptedPart != NULL)
        {
            memcpy_s(pLastEncryptedPart, *pulLastEncryptedPartLen, envelope.Data->EncryptedData.data, envelope.Data->EncryptedData.size);
            *pulLastEncryptedPartLen = (CK_ULONG)envelope.Data->EncryptedData.size;
        }
        else
        {
            *pulLastEncryptedPartLen = (CK_ULONG)envelope.Data->PullEncryptedDataLen;
        }
    }

    EncryptFinalEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DecryptInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DecryptInitRequest request;
    DecryptInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_GENERAL_ERROR;
    }

    request.KeyObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_DecryptInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    DecryptInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Decrypt)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pEncryptedData, CK_ULONG ulEncryptedDataLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen)
{
    LOG_ENTERING_TO_FUNCTION();
    if (NULL == pEncryptedData || NULL == pulDataLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DecryptRequest request;
    DecryptEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.EncryptedData.data = (uint8_t*)pEncryptedData;
    request.EncryptedData.size = (size_t)ulEncryptedDataLen;
    request.IsDataPtrSet = pData != NULL;
    request.PullDataLen = (uint32_t)*pulDataLen;

    int rv = nmrpc_call_Decrypt(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pData != NULL)
        {
            memcpy_s(pData, *pulDataLen, envelope.Data->Data.data, envelope.Data->Data.size);
            *pulDataLen = (CK_ULONG)envelope.Data->Data.size;
        }
        else
        {
            *pulDataLen = (CK_ULONG)envelope.Data->PullDataLen;
        }
    }

    DecryptEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DecryptUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen)
{
    LOG_ENTERING_TO_FUNCTION();
    if (NULL == pEncryptedPart || NULL == pulPartLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DecryptUpdateRequest request;
    DecryptUpdateEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.EncryptedData.data = (uint8_t*)pEncryptedPart;
    request.EncryptedData.size = (size_t)ulEncryptedPartLen;
    request.IsDataPtrSet = pPart != NULL;
    request.PullDataLen = (uint32_t)*pulPartLen;

    int rv = nmrpc_call_DecryptUpdate(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pPart != NULL)
        {
            memcpy_s(pPart, *pulPartLen, envelope.Data->Data.data, envelope.Data->Data.size);
            *pulPartLen = (CK_ULONG)envelope.Data->Data.size;
        }
        else
        {
            *pulPartLen = (CK_ULONG)envelope.Data->PullDataLen;
        }
    }

    DecryptUpdateEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DecryptFinal)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pLastPart, CK_ULONG_PTR pulLastPartLen)
{
    LOG_ENTERING_TO_FUNCTION();
    if (NULL == pulLastPartLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DecryptFinalRequest request;
    DecryptFinalEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.IsDataPtrSet = pLastPart != NULL;
    request.PullDataLen = (uint32_t)*pulLastPartLen;

    int rv = nmrpc_call_DecryptFinal(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pLastPart != NULL)
        {
            memcpy_s(pLastPart, *pulLastPartLen, envelope.Data->Data.data, envelope.Data->Data.size);
            *pulLastPartLen = (CK_ULONG)envelope.Data->Data.size;
        }
        else
        {
            *pulLastPartLen = (CK_ULONG)envelope.Data->PullDataLen;
        }
    }

    DecryptFinalEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DigestInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DigestInitRequest request;
    DigestInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (NMRPC_OK != MechanismValue_Create(&request.Mechanism, pMechanism))
    {
        return CKR_GENERAL_ERROR;
    }

    int rv = nmrpc_call_DigestInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        MechanismValue_Destroy(&request.Mechanism);

        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    DigestInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Digest)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pData || 0 == ulDataLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DigestRequest request;
    DigestEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pData;
    request.Data.size = (size_t)ulDataLen;
    request.IsDigestPtrSet = pDigest != NULL;
    request.PulDigestLen = (uint32_t)*pulDigestLen;

    int rv = nmrpc_call_Digest(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pDigest != NULL)
        {
            memcpy_s(pDigest, *pulDigestLen, envelope.Data->Data->data, envelope.Data->Data->size);
            *pulDigestLen = (CK_ULONG)envelope.Data->Data->size;
        }
        else
        {
            *pulDigestLen = envelope.Data->PulDigestLen;
        }
    }

    DigestEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DigestUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pPart || 0 == ulPartLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DigestUpdateRequest request;
    DigestUpdateEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pPart;
    request.Data.size = (size_t)ulPartLen;

    int rv = nmrpc_call_DigestUpdate(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    DigestUpdateEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DigestKey)(CK_SESSION_HANDLE hSession, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    DigestKeyRequest request;
    DigestKeyEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.ObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_DigestKey(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    DigestKeyEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DigestFinal)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pDigest, CK_ULONG_PTR pulDigestLen)
{
    LOG_ENTERING_TO_FUNCTION();

    DigestFinalRequest request;
    DigestFinalEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.IsDigestPtrSet = pDigest != NULL;
    request.PulDigestLen = (uint32_t)*pulDigestLen;

    int rv = nmrpc_call_DigestFinal(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pDigest != NULL)
        {
            memcpy_s(pDigest, *pulDigestLen, envelope.Data->Data->data, envelope.Data->Data->size);
            *pulDigestLen = (CK_ULONG)envelope.Data->Data->size;
        }
        else
        {
            *pulDigestLen = envelope.Data->PulDigestLen;
        }
    }

    DigestFinalEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_SignInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SignInitRequest request;
    SignInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_GENERAL_ERROR;
    }

    request.KeyObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_SignInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    SignInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Sign)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    LOG_ENTERING_TO_FUNCTION();
    if (NULL == pData || NULL == pulSignatureLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SignRequest request;
    SignEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pData;
    request.Data.size = (size_t)ulDataLen;
    request.IsSignaturePtrSet = pSignature != NULL;
    request.PullSignatureLen = (uint32_t)*pulSignatureLen;

    int rv = nmrpc_call_Sign(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pSignature != NULL)
        {
            memcpy_s(pSignature, *pulSignatureLen, envelope.Data->Signature.data, envelope.Data->Signature.size);
            *pulSignatureLen = (CK_ULONG)envelope.Data->Signature.size;
        }
        else
        {
            *pulSignatureLen = (CK_ULONG)envelope.Data->PullSignatureLen;
        }
    }

    SignEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_SignUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pPart)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SignUpdateRequest request;
    SignUpdateEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pPart;
    request.Data.size = (size_t)ulPartLen;

    int rv = nmrpc_call_SignUpdate(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    SignUpdateEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_SignFinal)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    LOG_ENTERING_TO_FUNCTION();

    SignFinalRequest request;
    SignFinalEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.IsSignaturePtrSet = pSignature != NULL;
    request.PullSignatureLen = (uint32_t)*pulSignatureLen;

    int rv = nmrpc_call_SignFinal(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pSignature != NULL)
        {
            memcpy_s(pSignature, *pulSignatureLen, envelope.Data->Signature.data, envelope.Data->Signature.size);
            *pulSignatureLen = (CK_ULONG)envelope.Data->Signature.size;
        }
        else
        {
            *pulSignatureLen = envelope.Data->PullSignatureLen;
        }
    }

    SignFinalEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_SignRecoverInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SignRecoverInitRequest request;
    SignRecoverInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_GENERAL_ERROR;
    }

    request.KeyObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_SignRecoverInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    SignRecoverInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_SignRecover)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG_PTR pulSignatureLen)
{
    LOG_ENTERING_TO_FUNCTION();
    if (NULL == pData || NULL == pulSignatureLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SignRecoverRequest request;
    SignRecoverEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pData;
    request.Data.size = (size_t)ulDataLen;
    request.IsSignaturePtrSet = pSignature != NULL;
    request.PullSignatureLen = (uint32_t)*pulSignatureLen;

    int rv = nmrpc_call_SignRecover(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pSignature != NULL)
        {
            memcpy_s(pSignature, *pulSignatureLen, envelope.Data->Signature.data, envelope.Data->Signature.size);
            *pulSignatureLen = (CK_ULONG)envelope.Data->Signature.size;
        }
        else
        {
            *pulSignatureLen = (CK_ULONG)envelope.Data->PullSignatureLen;
        }
    }

    SignRecoverEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_VerifyInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    VerifyInitRequest request;
    VerifyInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_MECHANISM_INVALID;
    }

    request.KeyObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_VerifyInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    VerifyInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_Verify)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pData, CK_ULONG ulDataLen, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pData || 0 == ulDataLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    if (NULL == pSignature || 0 == ulSignatureLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    VerifyRequest request;
    VerifyEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pData;
    request.Data.size = (size_t)ulDataLen;
    request.Signature.data = (uint8_t*)pSignature;
    request.Signature.size = (size_t)ulSignatureLen;

    int rv = nmrpc_call_Verify(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    VerifyEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_VerifyUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pPart, CK_ULONG ulPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pPart)
    {
        return CKR_ARGUMENTS_BAD;
    }

    VerifyUpdateRequest request;
    VerifyUpdateEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Data.data = (uint8_t*)pPart;
    request.Data.size = (size_t)ulPartLen;


    int rv = nmrpc_call_VerifyUpdate(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    VerifyUpdateEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_VerifyFinal)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen)
{
    LOG_ENTERING_TO_FUNCTION();


    if (NULL == pSignature || 0 == ulSignatureLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    VerifyFinalRequest request;
    VerifyFinalEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Signature.data = (uint8_t*)pSignature;
    request.Signature.size = (size_t)ulSignatureLen;

    int rv = nmrpc_call_VerifyFinal(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    VerifyFinalEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_VerifyRecoverInit)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hKey)
{
    LOG_ENTERING_TO_FUNCTION();

    VerifyRecoverInitRequest request;
    VerifyRecoverInitEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_MECHANISM_INVALID;
    }

    request.KeyObjectHandle = (uint32_t)hKey;

    int rv = nmrpc_call_VerifyRecoverInit(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    MechanismValue_Destroy(&request.Mechanism);
    VerifyRecoverInitEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_VerifyRecover)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pSignature, CK_ULONG ulSignatureLen, CK_BYTE_PTR pData, CK_ULONG_PTR pulDataLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pulDataLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    if (NULL == pSignature || 0 == ulSignatureLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    VerifyRecoverRequest request;
    VerifyRecoverEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Signature.data = (uint8_t*)pSignature;
    request.Signature.size = (size_t)ulSignatureLen;
    request.IsPtrDataSet = pData != NULL;
    request.PulDataLen = (uint32_t)*pulDataLen;

    int rv = nmrpc_call_VerifyRecover(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pData != NULL)
        {
            memcpy_s(pData, *pulDataLen, envelope.Data->Data.data, envelope.Data->Data.size);
            *pulDataLen = (CK_ULONG)envelope.Data->Data.size;
        }
        else
        {
            *pulDataLen = (CK_ULONG)envelope.Data->PulDataLen;
        }
    }

    VerifyRecoverEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_DigestEncryptUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_DecryptDigestUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_SignEncryptUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pPart, CK_ULONG ulPartLen, CK_BYTE_PTR pEncryptedPart, CK_ULONG_PTR pulEncryptedPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_DecryptVerifyUpdate)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pEncryptedPart, CK_ULONG ulEncryptedPartLen, CK_BYTE_PTR pPart, CK_ULONG_PTR pulPartLen)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_SUPPORTED;
}


CK_DEFINE_FUNCTION(CK_RV, C_GenerateKey)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount, CK_OBJECT_HANDLE_PTR phKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism || NULL == pTemplate || NULL == phKey)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GenerateKeyRequest request;
    GenerateKeyEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;
    AttrValueFromNative* attrTemplate = NULL;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_MECHANISM_INVALID;
    }

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulCount);
    if (NULL == attrTemplate)
    {
        MechanismValue_Destroy(&request.Mechanism);
        return CKR_GENERAL_ERROR;
    }

    request.Template.array = attrTemplate;
    request.Template.length = (int)ulCount;

    int rv = nmrpc_call_GenerateKey(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();

        MechanismValue_Destroy(&request.Mechanism);
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulCount);
        }

        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phKey = (CK_OBJECT_HANDLE)envelope.Data->KeyHandle;
    }

    MechanismValue_Destroy(&request.Mechanism);
    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulCount);
    }

    GenerateKeyEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GenerateKeyPair)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_ATTRIBUTE_PTR pPublicKeyTemplate, CK_ULONG ulPublicKeyAttributeCount, CK_ATTRIBUTE_PTR pPrivateKeyTemplate, CK_ULONG ulPrivateKeyAttributeCount, CK_OBJECT_HANDLE_PTR phPublicKey, CK_OBJECT_HANDLE_PTR phPrivateKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (pMechanism == NULL_PTR)
    {
        return CKR_ARGUMENTS_BAD;
    }

    if ((pPublicKeyTemplate == NULL_PTR && ulPublicKeyAttributeCount != 0) || (pPrivateKeyTemplate == NULL_PTR && ulPrivateKeyAttributeCount != 0))
    {
        return CKR_ARGUMENTS_BAD;
    }

    if (NULL == phPublicKey || NULL == phPrivateKey)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GenerateKeyPairRequest request;
    GenerateKeyPairEnvelope envelope;
    AttrValueFromNative* pubKeyAttrTemplate = NULL;
    AttrValueFromNative* privKeyAttrTemplate = NULL;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_GENERAL_ERROR;
    }

    pubKeyAttrTemplate = ConvertToAttrValueFromNative(pPublicKeyTemplate, ulPublicKeyAttributeCount);
    if (NULL == pubKeyAttrTemplate)
    {
        MechanismValue_Destroy(&request.Mechanism);
        return CKR_GENERAL_ERROR;
    }

    privKeyAttrTemplate = ConvertToAttrValueFromNative(pPrivateKeyTemplate, ulPrivateKeyAttributeCount);
    if (NULL == privKeyAttrTemplate)
    {
        MechanismValue_Destroy(&request.Mechanism);
        if (pubKeyAttrTemplate != NULL)
        {
            AttrValueFromNative_Destroy(pubKeyAttrTemplate, ulPublicKeyAttributeCount);
        }

        return CKR_GENERAL_ERROR;
    }

    request.PublicKeyTemplate.array = pubKeyAttrTemplate;
    request.PublicKeyTemplate.length = (int)ulPublicKeyAttributeCount;
    request.PrivateKeyTemplate.array = privKeyAttrTemplate;
    request.PrivateKeyTemplate.length = (int)ulPrivateKeyAttributeCount;

    int rv = nmrpc_call_GenerateKeyPair(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        MechanismValue_Destroy(&request.Mechanism);
        AttrValueFromNative_Destroy(pubKeyAttrTemplate, ulPublicKeyAttributeCount);
        AttrValueFromNative_Destroy(privKeyAttrTemplate, ulPrivateKeyAttributeCount);

        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phPublicKey = (CK_OBJECT_HANDLE)envelope.Data->PublicKeyHandle;
        *phPrivateKey = (CK_OBJECT_HANDLE)envelope.Data->PrivateKeyHandle;
    }

    MechanismValue_Destroy(&request.Mechanism);
    AttrValueFromNative_Destroy(pubKeyAttrTemplate, ulPublicKeyAttributeCount);
    AttrValueFromNative_Destroy(privKeyAttrTemplate, ulPrivateKeyAttributeCount);
    GenerateKeyPairEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_WrapKey)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hWrappingKey, CK_OBJECT_HANDLE hKey, CK_BYTE_PTR pWrappedKey, CK_ULONG_PTR pulWrappedKeyLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (pMechanism == NULL_PTR || pulWrappedKeyLen == NULL)
    {
        return CKR_ARGUMENTS_BAD;
    }

    WrapKeyRequest request;
    WrapKeyEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_MECHANISM_INVALID;
    }

    request.WrappingKeyHandle = (uint32_t)hWrappingKey;
    request.KeyHandle = (uint32_t)hKey;
    request.IsPtrWrappedKeySet = pWrappedKey != NULL;
    request.PulWrappedKeyLen = (uint32_t)*pulWrappedKeyLen;

    int rv = nmrpc_call_WrapKey(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();

        MechanismValue_Destroy(&request.Mechanism);
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pWrappedKey != NULL)
        {
            memcpy_s(pWrappedKey, *pulWrappedKeyLen, envelope.Data->WrappedKeyData.data, envelope.Data->WrappedKeyData.size);
            *pulWrappedKeyLen = (CK_ULONG)envelope.Data->WrappedKeyData.size;
        }
        else
        {
            *pulWrappedKeyLen = (CK_ULONG)envelope.Data->PulWrappedKeyLen;
        }
    }

    MechanismValue_Destroy(&request.Mechanism);
    WrapKeyEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_UnwrapKey)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hUnwrappingKey, CK_BYTE_PTR pWrappedKey, CK_ULONG ulWrappedKeyLen, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism || NULL == pTemplate || NULL == pWrappedKey || NULL == phKey)
    {
        return CKR_ARGUMENTS_BAD;
    }

    UnwrapKeyRequest request;
    UnwrapKeyEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;
    AttrValueFromNative* attrTemplate = NULL;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_MECHANISM_INVALID;
    }

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulAttributeCount);
    if (NULL == attrTemplate)
    {
        MechanismValue_Destroy(&request.Mechanism);
        return CKR_GENERAL_ERROR;
    }

    request.Template.array = attrTemplate;
    request.Template.length = (int)ulAttributeCount;
    request.UnwrappingKeyHandle = (uint32_t)hUnwrappingKey;
    request.WrappedKeyData.data = (uint8_t*)pWrappedKey;
    request.WrappedKeyData.size = (size_t)ulWrappedKeyLen;

    int rv = nmrpc_call_UnwrapKey(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        MechanismValue_Destroy(&request.Mechanism);
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulAttributeCount);
        }

        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phKey = (CK_OBJECT_HANDLE)envelope.Data->KeyHandle;
    }

    MechanismValue_Destroy(&request.Mechanism);
    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulAttributeCount);
    }

    UnwrapKeyEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}

CK_DEFINE_FUNCTION(CK_RV, C_DeriveKey)(CK_SESSION_HANDLE hSession, CK_MECHANISM_PTR pMechanism, CK_OBJECT_HANDLE hBaseKey, CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulAttributeCount, CK_OBJECT_HANDLE_PTR phKey)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pMechanism || NULL == pTemplate || NULL == phKey)
    {
        return CKR_ARGUMENTS_BAD;
    }

    DeriveKeyRequest request;
    DeriveKeyEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;
    AttrValueFromNative* attrTemplate = NULL;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    if (MechanismValue_Create(&request.Mechanism, pMechanism) != NMRPC_OK)
    {
        return CKR_MECHANISM_INVALID;
    }

    request.BaseKeyHandle = (uint32_t)hBaseKey;

    attrTemplate = ConvertToAttrValueFromNative(pTemplate, ulAttributeCount);
    if (NULL == attrTemplate)
    {
        MechanismValue_Destroy(&request.Mechanism);
        return CKR_GENERAL_ERROR;
    }

    request.Template.array = attrTemplate;
    request.Template.length = (int)ulAttributeCount;

    int rv = nmrpc_call_DeriveKey(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        MechanismValue_Destroy(&request.Mechanism);
        if (NULL != attrTemplate)
        {
            AttrValueFromNative_Destroy(attrTemplate, ulAttributeCount);
        }

        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        *phKey = (CK_OBJECT_HANDLE)envelope.Data->KeyHandle;
    }

    MechanismValue_Destroy(&request.Mechanism);
    if (NULL != attrTemplate)
    {
        AttrValueFromNative_Destroy(attrTemplate, ulAttributeCount);
    }

    DeriveKeyEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_SeedRandom)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR pSeed, CK_ULONG ulSeedLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == pSeed || 0 == ulSeedLen)
    {
        return CKR_ARGUMENTS_BAD;
    }

    SeedRandomRequest request;
    SeedRandomEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.Seed.data = pSeed;
    request.Seed.size = ulSeedLen;

    int rv = nmrpc_call_SeedRandom(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    SeedRandomEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GenerateRandom)(CK_SESSION_HANDLE hSession, CK_BYTE_PTR RandomData, CK_ULONG ulRandomLen)
{
    LOG_ENTERING_TO_FUNCTION();

    if (NULL == RandomData)
    {
        return CKR_ARGUMENTS_BAD;
    }

    GenerateRandomRequest request;
    GenerateRandomEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }
    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.SessionId = (uint32_t)hSession;
    request.RandomLen = (uint32_t)ulRandomLen;

    int rv = nmrpc_call_GenerateRandom(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        memcpy(RandomData, envelope.Data->data, envelope.Data->size);
    }

    GenerateRandomEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}


CK_DEFINE_FUNCTION(CK_RV, C_GetFunctionStatus)(CK_SESSION_HANDLE hSession)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_PARALLEL;
}


CK_DEFINE_FUNCTION(CK_RV, C_CancelFunction)(CK_SESSION_HANDLE hSession)
{
    LOG_ENTERING_TO_FUNCTION();

    return CKR_FUNCTION_NOT_PARALLEL;
}


CK_DEFINE_FUNCTION(CK_RV, C_WaitForSlotEvent)(CK_FLAGS flags, CK_SLOT_ID_PTR pSlot, CK_VOID_PTR pReserved)
{
    LOG_ENTERING_TO_FUNCTION();

    WaitForSlotEventRequest request;
    WaitForSlotEventEnvelope envelope;

    nmrpc_global_context_t ctx;
    SockContext_t tcp;

    if (P11SocketInit(&tcp) != NMRPC_OK)
    {
        return CKR_DEVICE_ERROR;
    }

    nmrpc_global_context_tcp_init(&ctx, &tcp);
    InitCallContext(&ctx, &request.AppId);

    request.Flags = (uint32_t)flags;
    request.IsSlotPtrSet = pSlot != NULL;
    request.IsReservedPtrSet = pReserved != NULL;

    int rv = nmrpc_call_WaitForSlotEvent(&ctx, &request, &envelope);
    if (rv != NMRPC_OK)
    {
        LOG_FAILED_CALL_RPC();
        return CKR_DEVICE_ERROR;
    }

    if ((CK_RV)envelope.Rv == CKR_OK)
    {
        if (pSlot == NULL)
        {
            return CKR_ARGUMENTS_BAD;
        }

        *pSlot = (CK_SLOT_ID)envelope.Data->SlotId;
    }

    WaitForSlotEventEnvelope_Release(&envelope);

    return (CK_RV)envelope.Rv;
}
