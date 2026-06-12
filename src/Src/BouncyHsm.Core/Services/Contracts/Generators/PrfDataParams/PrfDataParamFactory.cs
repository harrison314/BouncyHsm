using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal static class PrfDataParamFactory
{
    public static async Task<IPrfDataParam[]> Create(Ckp_CkSp800_108PrfDataParsms[] dataParams,
        IP11HwServices hwServices,
        IMemorySession memorySession,
        IP11Session p11Session,
        CKM alowedMechanism,
        CancellationToken cancellationToken)
    {
        if (dataParams.Length == 0)
        {
            return Array.Empty<IPrfDataParam>();
        }

        IPrfDataParam[] result = new IPrfDataParam[dataParams.Length];
        for (int i = 0; i < result.Length; i++)
        {
            Ckp_CkSp800_108PrfDataParsms dataParam = dataParams[i];
            result[i] = ((CK_PRF_DATA_TYPE)dataParam.Type) switch
            {
                CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE => new IterationVariablePrfDataParam(dataParam.LittleEndian, (int)dataParam.WidthInBits),
                CK_PRF_DATA_TYPE.CK_SP800_108_COUNTER => new CounterPrfDataParam(dataParam.LittleEndian, (int)dataParam.WidthInBits),
                CK_PRF_DATA_TYPE.CK_SP800_108_BYTE_ARRAY => new ByteArrayPrfDataParam(dataParam.Value ?? throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, "pValue is null for CK_SP800_108_BYTE_ARRAY")),
                CK_PRF_DATA_TYPE.CK_SP800_108_DKM_LENGTH => new DkmLengthPrfDataParam(dataParam.LittleEndian, (int)dataParam.WidthInBits, ConvertLengthMethod(dataParam.LengthMethod)),
                CK_PRF_DATA_TYPE.CK_SP800_108_KEY_HANDLE => new KeyHandlePrfDataParam(await FindDeriveKey(hwServices, memorySession, p11Session, alowedMechanism, dataParam, cancellationToken)),
                _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"type has invalid value ")
            };
        }

        return result;
    }

    private static async Task<SecretKeyObject> FindDeriveKey(IP11HwServices hwServices, IMemorySession memorySession, IP11Session p11Session, CKM alowedMechanism, Ckp_CkSp800_108PrfDataParsms dataParam, CancellationToken cancellationToken)
    {
        SecretKeyObject keyObject = await hwServices.FindObjectByHandle<SecretKeyObject>(memorySession, p11Session, dataParam.KeyHandle, cancellationToken);

        if (!keyObject.CkaDerive)
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The derive operation is not allowed because object is not authorized to derive key (CKA_DERVIVE must be true). Key is in mechanism parameters.");
        }

        if (!keyObject.MechanismIsAllowed(alowedMechanism))
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                $"The mechanism {alowedMechanism} is not allowed in CKA_ALLOWED_MECHANISMS on key object. Key is in mechanism parameters.");
        }

        return keyObject;
    }

    private static CK_SP800_108_DKM_LENGTH_METHOD ConvertLengthMethod(uint lengthMethod)
    {
        CK_SP800_108_DKM_LENGTH_METHOD value = (CK_SP800_108_DKM_LENGTH_METHOD)lengthMethod;
        if (!Enum.IsDefined<CK_SP800_108_DKM_LENGTH_METHOD>(value))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Data parameter is invalid value of CK_SP800_108_DKM_LENGTH_METHOD, value {lengthMethod}.");
        }

        return value;
    }
}
