using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Net.Pkcs11Interop.LowLevelAPI80;
using Net.Pkcs11Interop.HighLevelAPI80;
using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI80;

internal class SessionExtensions : ISessionExtensions
{
    delegate NativeULong C_SessionCancelDelegate(NativeULong sessionId, NativeULong flags);

    /*
     CK_DECLARE_FUNCTION(CK_RV, C_EncapsulateKey)(
       CK_SESSION_HANDLE hSession,
       CK_MECHANISM_PTR pMechanism,
       CK_OBJECT_HANDLE hPublicKey,
       CK_ATTRIBUTE_PTR pTemplate,
       CK_ULONG ulAttributeCount,
       CK_BYTE_PTR pCiphertext,
       CK_ULONG_PTR pulCiphertextLen,
       CK_OBJECT_HANDLE_PTR phKey
     );
    */
    delegate NativeULong C_EncapsulateKeyDelegate(NativeULong hSession,
        in CK_MECHANISM pMechanism,
        NativeULong hPublicKey,
        CK_ATTRIBUTE[] pTemplate,
        NativeULong ulAttributeCount,
        IntPtr pCiphertext,
        ref NativeULong pulCiphertextLen,
        ref NativeULong phKey);
    /*
      CK_DECLARE_FUNCTION(CK_RV, C_DecapsulateKey)(
        CK_SESSION_HANDLE hSession,
        CK_MECHANISM_PTR pMechanism,
        CK_OBJECT_HANDLE hPrivateKey,
        CK_ATTRIBUTE_PTR pTemplate,
        CK_ULONG ulAttributeCount,
        CK_BYTE_PTR pCiphertext,
        CK_ULONG ulCiphertextLen,
        CK_OBJECT_HANDLE_PTR phKey,
      );
     */
    delegate NativeULong C_DecapsulateKeyDelegate(NativeULong hSession,
        in CK_MECHANISM pMechanism,
        NativeULong hPrivateKey,
        CK_ATTRIBUTE[] pTemplate,
        NativeULong ulAttributeCount,
        IntPtr pCiphertext,
        NativeULong ulCiphertextLen,
        ref NativeULong phKey);

    /*
     CK_DECLARE_FUNCTION(CK_RV, C_GetSessionValidationFlags)(
       CK_SESSION_HANDLE hSession,
       CK_SESSION_VALIDATION_FLAGS_TYPE type,
       CK_FLAGS_PTR pFlags,
       );
     */
    delegate NativeULong C_GetSessionValidationFlagsDelegate(NativeULong hSession, NativeULong type, ref NativeULong pFlags);


    private C_SessionCancelDelegate C_SessionCancel;
    private C_EncapsulateKeyDelegate C_EncapsulateKey;
    private C_DecapsulateKeyDelegate C_DecapsulateKey;
    private C_GetSessionValidationFlagsDelegate C_GetSessionValidationFlags;

    public SessionExtensions(IntPtr lirarayhandle)
    {
        this.C_SessionCancel = this.GetDelegate<C_SessionCancelDelegate>(lirarayhandle, "C_SessionCancel");
        this.C_EncapsulateKey = this.GetDelegate<C_EncapsulateKeyDelegate>(lirarayhandle, "C_EncapsulateKey");
        this.C_DecapsulateKey = this.GetDelegate<C_DecapsulateKeyDelegate>(lirarayhandle, "C_DecapsulateKey");
        this.C_GetSessionValidationFlags = this.GetDelegate<C_GetSessionValidationFlagsDelegate>(lirarayhandle, "C_GetSessionValidationFlags");
    }

    public void SessionCancel(ISession session, uint CkfFlags)
    {
        NativeULong rvRaw = this.C_SessionCancel((NativeULong)session.SessionId, (NativeULong)CkfFlags);
        if ((CKR)rvRaw != CKR.CKR_OK)
        {
            throw new Pkcs11Exception("C_SessionCancel", (CKR)rvRaw);
        }
    }

    public void EncapsulateKey(ISession session,
        IMechanism mechanism,
        IObjectHandle publicKeyHandle,
        List<IObjectAttribute> template,
        out byte[] ciphertext,
        out IObjectHandle phKey)
    {
        NativeULong ckr;
        NativeULong pulCiphertextLen = 0;
        NativeULong phKeyHandle = 0;
        IntPtr ciphertextPtr = IntPtr.Zero;

        try
        {
            CK_MECHANISM mechanismStruct = (CK_MECHANISM)mechanism.ToMarshalableStructure();
            CK_ATTRIBUTE[] templateArray = this.ProcessAttributes(template);
            ckr = this.C_EncapsulateKey((NativeULong)session.SessionId,
                in mechanismStruct,
                (NativeULong)publicKeyHandle.ObjectId,
                templateArray,
                (NativeULong)templateArray.Length,
                IntPtr.Zero,
                ref pulCiphertextLen,
                ref phKeyHandle);

            if (ckr != (NativeULong)CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_EncapsulateKey", (CKR)ckr);
            }

            ciphertextPtr = MemoryUtils.MemAlloc((uint)pulCiphertextLen);

            ckr = this.C_EncapsulateKey((NativeULong)session.SessionId,
                in mechanismStruct,
                (NativeULong)publicKeyHandle.ObjectId,
                templateArray,
                (NativeULong)templateArray.Length,
                ciphertextPtr,
                ref pulCiphertextLen,
                ref phKeyHandle);

            if (ckr != (NativeULong)CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_EncapsulateKey", (CKR)ckr);
            }

            ciphertext = MemoryUtils.MemDupFromPtr(ciphertextPtr, (uint)pulCiphertextLen);
            phKey = new ObjectHandle(phKeyHandle);
        }
        finally
        {
            MemoryUtils.MemFreeReset(ref ciphertextPtr);
        }
    }

    public void DecapsulateKey(ISession session,
        IMechanism mechanism,
        IObjectHandle publicKeyHandle,
        List<IObjectAttribute> template,
        byte[] ciphertext,
        out IObjectHandle phKey)
    {
        NativeULong ckr;
        NativeULong phKeyHandle = 0;
        IntPtr ciphertextPtr = IntPtr.Zero;

        try
        {
            CK_MECHANISM mechanismStruct = (CK_MECHANISM)mechanism.ToMarshalableStructure();
            CK_ATTRIBUTE[] templateArray = this.ProcessAttributes(template);

            ciphertextPtr = MemoryUtils.MemDup(ciphertext);

            ckr = this.C_DecapsulateKey((NativeULong)session.SessionId,
                in mechanismStruct,
                (NativeULong)publicKeyHandle.ObjectId,
                templateArray,
                (NativeULong)templateArray.Length,
                ciphertextPtr,
                (NativeULong)ciphertext.Length,
                ref phKeyHandle);

            if (ckr != (NativeULong)CKR.CKR_OK)
            {
                throw new Pkcs11Exception("C_DecapsulateKey", (CKR)ckr);
            }

            phKey = new ObjectHandle(phKeyHandle);
        }
        finally
        {
            MemoryUtils.MemFreeReset(ref ciphertextPtr);
        }
    }

    public ulong GetSessionValidationFlags(ISession session, uint type)
    {
        NativeULong flags = 0;
        NativeULong rvRaw = this.C_GetSessionValidationFlags((NativeULong)session.SessionId, (NativeULong)type, ref flags);
        if ((CKR)rvRaw != CKR.CKR_OK)
        {
            throw new Pkcs11Exception("C_SessionCancel", (CKR)rvRaw);
        }

        return Convert.ToUInt64(flags);
    }

    private CK_ATTRIBUTE[] ProcessAttributes(List<IObjectAttribute> template)
    {
        CK_ATTRIBUTE[] attrs = new CK_ATTRIBUTE[template.Count];
        for (int i = 0; i < template.Count; i++)
        {
            attrs[i] = (CK_ATTRIBUTE)template[i].ToMarshalableStructure();
        }

        return attrs;
    }

    private TDelegate GetDelegate<TDelegate>(IntPtr lirarayhandle, string functionName)
        where TDelegate : Delegate
    {
        IntPtr procAddress = NativeLibrary.GetExport(lirarayhandle, functionName);
        if (procAddress == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Cannot get address of {functionName} function");
        }
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(procAddress);
    }
}
