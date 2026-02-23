using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// CK_ML_DSA_PARAMETER_SET_TYPE
/// </summary>
public enum CK_ML_DSA_PARAMETER_SET : uint
{
    /// <summary>
    /// ML-DSA 44-bit parameter set
    /// </summary>
    CKP_ML_DSA_44 = 0x00000001,

    /// <summary>
    /// ML-DSA 65-bit parameter set
    /// </summary>
    CKP_ML_DSA_65 = 0x00000002,

    /// <summary>
    /// ML-DSA 87-bit parameter set
    /// </summary>
    CKP_ML_DSA_87 = 0x00000003
}
