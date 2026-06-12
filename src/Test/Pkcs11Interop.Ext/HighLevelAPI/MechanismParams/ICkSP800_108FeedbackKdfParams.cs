using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

public interface ICkSP800_108FeedbackKdfParams : IMechanismParams
{
    IObjectHandle[] GetAdditionalKeyhandlers();
}