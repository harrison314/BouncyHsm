using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.Contracts;

public class InvalidAttributeTypeCastException : ApplicationException
{
    public InvalidAttributeTypeCastException()
    {

    }

    public InvalidAttributeTypeCastException(AttrTypeTag exceptedType, [CallerMemberName] string fnName = "")
        : base($"Can not convert value of type {exceptedType} using method {fnName}.")
    {

    }

    public InvalidAttributeTypeCastException(string message)
        : base(message)
    {

    }
}