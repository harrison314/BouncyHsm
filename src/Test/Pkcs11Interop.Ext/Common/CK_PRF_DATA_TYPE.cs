namespace Pkcs11Interop.Ext.Common;

public static class CK_PRF_DATA_TYPE
{
    public const uint CK_SP800_108_ITERATION_VARIABLE = 0x00000001;
    public const uint CK_SP800_108_OPTIONAL_COUNTER = 0x00000002;
    public const uint CK_SP800_108_DKM_LENGTH = 0x00000003;
    public const uint CK_SP800_108_BYTE_ARRAY = 0x00000004;
    public const uint CK_SP800_108_COUNTER = CK_SP800_108_OPTIONAL_COUNTER;
    public const uint CK_SP800_108_KEY_HANDLE = 0x00000005;
}