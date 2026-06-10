using Dunet;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pkcs11Interop.Ext.Common;

[Union]
public partial record KdfDataParam
{
    partial record IterationVariable(int WidthInBits, bool LittleEndian);
    partial record Counter(int WidthInBits, bool LittleEndian);
    partial record DkmLength(int WidthInBits, bool LittleEndian, uint DkmLengthMethod);
    partial record KeyHandle(IObjectHandle ObjectHandle);
    partial record ByteArray(byte[] Bytes);
}
