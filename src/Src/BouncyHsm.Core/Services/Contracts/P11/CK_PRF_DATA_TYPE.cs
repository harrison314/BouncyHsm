using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.P11;

public enum CK_PRF_DATA_TYPE : uint
{
    CK_SP800_108_ITERATION_VARIABLE = 0x00000001,
   // CK_SP800_108_OPTIONAL_COUNTER = 0x00000002,
    CK_SP800_108_DKM_LENGTH = 0x00000003,
    CK_SP800_108_BYTE_ARRAY = 0x00000004,
    CK_SP800_108_COUNTER = 0x00000002, // equals to CK_SP800_108_OPTIONAL_COUNTER
    CK_SP800_108_KEY_HANDLE = 0x00000005,
}
