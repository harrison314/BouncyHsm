using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

/*
 typedef struct CK_HKDF_PARAMS {
CK_BBOOL bExtract;
CK_BBOOL bExpand;
CK_MECHANISM_TYPE prfHashMechanism;
CK_ULONG ulSaltType;
CK_BYTE_PTR pSalt;
CK_ULONG ulSaltLen;
CK_OBJECT_HANDLE hSaltKey;
CK_BYTE_PTR pInfo;
CK_ULONG ulInfoLen;
} CK_HKDF_PARAMS;
*/
public interface ICkSalsa20Params : IMechanismParams
{
   
}
