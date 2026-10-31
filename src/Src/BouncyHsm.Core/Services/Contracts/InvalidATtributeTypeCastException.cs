using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.Contracts;

public class InvalidAttributeTypeCastException : RpcPkcs11Exception
{
    public InvalidAttributeTypeCastException(AttrTypeTag exceptedType, [CallerMemberName] string fnName = "")
        : base(P11.CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Can not convert value of type {exceptedType} using method {fnName}.")
    {

    }

    public InvalidAttributeTypeCastException(string message)
        : base(P11.CKR.CKR_ATTRIBUTE_VALUE_INVALID, message)
    {

    }
}