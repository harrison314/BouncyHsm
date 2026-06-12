using Microsoft.VisualBasic;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.Common;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

internal class CkSP800_108KdfParams : ICkSP800_108KdfParams
{
    private bool disposedValue;

    private CK_SP800_108_KDF_PARAMS lowLevelStruct = new CK_SP800_108_KDF_PARAMS();
    private List<CK_PRF_DATA_PARAM> dataParsms = new List<CK_PRF_DATA_PARAM>();

    public CkSP800_108KdfParams(CKM pdfType, KdfDataParam[] additionalParams)
    {
        this.lowLevelStruct.pAdditionalDerivedKeys = IntPtr.Zero;
        this.lowLevelStruct.ulAdditionalDerivedKeys = 0;

        this.lowLevelStruct.prfType = (uint)pdfType;

        foreach (KdfDataParam kdfParam in additionalParams)
        {
            kdfParam.Match(
                iteration => this.AddIteration(this.dataParsms, iteration),
                counter => this.AddCounter(this.dataParsms, counter),
                dkmLength => this.AddDkmLength(this.dataParsms, dkmLength),
                keyHandle => this.AddObjectHandle(this.dataParsms, keyHandle),
                byteArray => this.AddByteArray(this.dataParsms, byteArray));
        }

        this.lowLevelStruct.ulNumberOfDataParams = 0;
        this.lowLevelStruct.pDataParams = IntPtr.Zero;

        if (this.dataParsms.Count > 0)
        {
            this.lowLevelStruct.ulNumberOfDataParams = (uint)this.dataParsms.Count;
            this.lowLevelStruct.pDataParams = MemoryUtils.MemDup(this.dataParsms);
        }
    }

    private unsafe void AddIteration(List<CK_PRF_DATA_PARAM> dataParsms, KdfDataParam.IterationVariable iterationVariable)
    {
        CK_SP800_108_COUNTER_FORMAT format = new CK_SP800_108_COUNTER_FORMAT()
        {
            bLittleEndian = iterationVariable.LittleEndian ? (byte)1 : (byte)0,
            ulWidthInBits = (uint)iterationVariable.WidthInBits
        };

        CK_PRF_DATA_PARAM prfDataValue = new CK_PRF_DATA_PARAM()
        {
            type = CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE,
            pValue = MemoryUtils.MemDup(ref format),
            ulValueLen = (uint)sizeof(CK_SP800_108_COUNTER_FORMAT)
        };

        dataParsms.Add(prfDataValue);
    }

    private unsafe void AddCounter(List<CK_PRF_DATA_PARAM> dataParsms, KdfDataParam.Counter counter)
    {
        CK_SP800_108_COUNTER_FORMAT format = new CK_SP800_108_COUNTER_FORMAT()
        {
            bLittleEndian = counter.LittleEndian ? (byte)1 : (byte)0,
            ulWidthInBits = (uint)counter.WidthInBits
        };

        CK_PRF_DATA_PARAM prfDataValue = new CK_PRF_DATA_PARAM()
        {
            type = CK_PRF_DATA_TYPE.CK_SP800_108_COUNTER,
            pValue = MemoryUtils.MemDup(ref format),
            ulValueLen = (uint)sizeof(CK_SP800_108_COUNTER_FORMAT)
        };

        dataParsms.Add(prfDataValue);
    }

    private void AddByteArray(List<CK_PRF_DATA_PARAM> dataParsms, KdfDataParam.ByteArray byteArray)
    {
        byte[] data = byteArray.Bytes;
        if (data.Length > 0)
        {
            CK_PRF_DATA_PARAM prfDataValue = new CK_PRF_DATA_PARAM()
            {
                type = CK_PRF_DATA_TYPE.CK_SP800_108_BYTE_ARRAY,
                pValue = MemoryUtils.MemDup(data),
                ulValueLen = (uint)data.Length
            };

            dataParsms.Add(prfDataValue);
        }
    }

    private void AddObjectHandle(List<CK_PRF_DATA_PARAM> dataParsms, KdfDataParam.KeyHandle keyHandle)
    {
        uint handle = Convert.ToUInt32(keyHandle.ObjectHandle.ObjectId);
        CK_PRF_DATA_PARAM prfDataValue = new CK_PRF_DATA_PARAM()
        {
            type = CK_PRF_DATA_TYPE.CK_SP800_108_KEY_HANDLE,
            pValue = MemoryUtils.MemDup(ref handle),
            ulValueLen = sizeof(uint)
        };

        dataParsms.Add(prfDataValue);
    }

    private unsafe void AddDkmLength(List<CK_PRF_DATA_PARAM> dataParsms, KdfDataParam.DkmLength dkmLength)
    {
        CK_SP800_108_DKM_LENGTH_FORMAT format = new CK_SP800_108_DKM_LENGTH_FORMAT()
        {
            bLittleEndian = dkmLength.LittleEndian ? (byte)1 : (byte)0,
            ulWidthInBits = (uint)dkmLength.WidthInBits,
            dkmLengthMethod = dkmLength.DkmLengthMethod
        };

        CK_PRF_DATA_PARAM prfDataValue = new CK_PRF_DATA_PARAM()
        {
            type = CK_PRF_DATA_TYPE.CK_SP800_108_DKM_LENGTH,
            pValue = MemoryUtils.MemDup(ref format),
            ulValueLen = (uint)sizeof(CK_SP800_108_DKM_LENGTH_FORMAT)
        };

        dataParsms.Add(prfDataValue);

    }

    public object ToMarshalableStructure()
    {
        if (this.disposedValue)
            throw new ObjectDisposedException(this.GetType().FullName);

        return this.lowLevelStruct;
    }

    public IObjectHandle[] GetAdditionalKeyhandlers()
    {
        throw new NotImplementedException("GetAdditionalKeyhandlers not implement");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pDataParams);

            foreach (CK_PRF_DATA_PARAM dataParam in this.dataParsms)
            {
                MemoryUtils.MemFree(dataParam.pValue);
            }

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~CkSP800_108KdfParams()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}
