using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.P11;

public enum CK_ML_KEM_PARAMETER_SET : uint
{
    CKP_ML_KEM_512 = 0x00000001,
    CKP_ML_KEM_768 = 0x00000002,
    CKP_ML_KEM_1024 = 0x00000003,
}
