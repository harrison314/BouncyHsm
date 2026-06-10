using Dunet;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pkcs11Interop.Ext.Common;

[Union]
public partial record KdfDataParam
{
    partial record IterationVariable(bool LittleEndian, int WidthInBits);
    partial record Counter(bool LittleEndian, int WidthInBits);
    partial record DkmLength(bool LittleEndian, int WidthInBits, uint DkmLengthMethod);
    partial record KeyHandle(IObjectHandle ObjectHandle);
    partial record ByteArray(byte[] Bytes);
}
