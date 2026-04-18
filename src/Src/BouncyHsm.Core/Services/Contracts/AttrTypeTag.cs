using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts;

public enum AttrTypeTag
{
    ByteArray = 0,
    String = 1,
    CkUint = 2,
    CkBool = 3,
    DateTime = 4,
    CkAttributeArray = 5,
    UintArray = 6,
}
