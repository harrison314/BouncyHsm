using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.P11;

public enum CK_HEDGE_TYPE : uint
{
    CKH_HEDGE_PREFERRED = 0x00000000,
    CKH_HEDGE_REQUIRED = 0x00000001,
    CKH_DETERMINISTIC_REQUIRED = 0x00000002,
}
