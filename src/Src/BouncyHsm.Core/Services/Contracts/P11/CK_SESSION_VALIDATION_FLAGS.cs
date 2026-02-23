using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.P11;

public enum CK_SESSION_VALIDATION_FLAGS : uint
{
    CKS_LAST_VALIDATION_OK = 0x00000001
}
