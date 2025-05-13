using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

/// <summary>
/// typedef struct CK_EDDSA_PARAMS {
/// CK_BBOOL phFlag;
/// CK_ULONG ulContextDataLen;
/// CK_BYTE_PTR pContextData;
/// } CK_EDDSA_PARAMS;
/// </summary>
public interface ICkEddsaParams : IMechanismParams
{
}
