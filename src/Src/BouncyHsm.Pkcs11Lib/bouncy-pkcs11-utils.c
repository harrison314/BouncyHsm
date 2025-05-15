#include <stdlib.h>
#include <string.h>
#include <ctype.h>

#include "bouncy-pkcs11.h"
#include "rpc/rpc.h"
#include "logger.h"

#include "bouncy-pkcs11-utils.h"

void SetPaddedStrSafe(CK_UTF8CHAR* destination, size_t destinationSize, const char* src)
{
    LOG_ENTERING_TO_FUNCTION();

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
    LOG_ENTERING_TO_FUNCTION();

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

AttrValueFromNative* ConvertToAttrValueFromNative(CK_ATTRIBUTE_PTR pTemplate, CK_ULONG ulCount)
{
    LOG_ENTERING_TO_FUNCTION();

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
    LOG_ENTERING_TO_FUNCTION();

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

int MechanismValue_Create(MechanismValue* value, CK_MECHANISM_PTR pMechanism)
{
    LOG_ENTERING_TO_FUNCTION();

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
            salsa20DeriveParams.BlockCounter = *((uint64_t*)salsa20Params->pBlockCounter);
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

        eddsaParamsDerivedparams.PhFlag = (bool)eddsaParams->phFlag;
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
    LOG_ENTERING_TO_FUNCTION();
    if (value == NULL)
    {
        log_message(LOG_LEVEL_TRACE, "Parameter value in MechanismValue_Destroy is NULL.");
        return;
    }

    if (value->MechanismParamMp != NULL)
    {
        Binary_Release(value->MechanismParamMp);
        free((void*)value->MechanismParamMp);
        value->MechanismParamMp = NULL;
    }
}