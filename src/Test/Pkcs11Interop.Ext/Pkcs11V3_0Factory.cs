using Net.Pkcs11Interop.Common;
using Pkcs11Interop.Ext.HighLevelAPI.Factories;

namespace Pkcs11Interop.Ext
{
    public class Pkcs11V3_0Factory
    {
        public static Pkcs11V3_0Factory Instance
        {
            get;
        } = new Pkcs11V3_0Factory();

        public IMechanismParamsV3Factory MechanismParamsFactory
        {
            get;
        }

        private Pkcs11V3_0Factory()
        {
            if (Platform.NativeULongSize == 4)
            {
                if (Platform.StructPackingSize == 0)
                {
                    this.MechanismParamsFactory = new HighLevelAPI40.MechanismParams.MechanismParamsV3Factory();
                }
                else
                {
                    this.MechanismParamsFactory = new HighLevelAPI41.MechanismParams.MechanismParamsV3Factory();
                }
            }
            else
            {
                if (Platform.StructPackingSize == 0)
                {
                    this.MechanismParamsFactory = new HighLevelAPI80.MechanismParams.MechanismParamsV3Factory();

                }
                else
                {
                    this.MechanismParamsFactory = new HighLevelAPI81.MechanismParams.MechanismParamsV3Factory();
                }
            }
        }
    }
}
