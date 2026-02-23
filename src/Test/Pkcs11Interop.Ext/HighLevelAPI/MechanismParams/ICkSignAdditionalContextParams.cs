using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

/// <summary>
/// typedef struct CK_SIGN_ADDITIONAL_CONTEXT {
///      CK_HEDGE_TYPE hedgeVariant;
///     CK_BYTE_PTR pContext;
///     CK_ULONG ulContextLen;
/// }
/// CK_SIGN_ADDITIONAL_CONTEXT;
/// </summary>
public interface ICkSignAdditionalContextParams : IMechanismParams
{
}