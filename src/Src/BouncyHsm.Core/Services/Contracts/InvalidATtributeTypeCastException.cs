using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.Contracts;

public class InvalidATtributeTypeCastException : ApplicationException
{
    public InvalidATtributeTypeCastException()
    {

    }

    public InvalidATtributeTypeCastException(AttrTypeTag exceptedType, [CallerMemberName] string fnName = "")
        : base($"Cannot call convert value type {exceptedType} using method {fnName}.")
    {

    }

    public InvalidATtributeTypeCastException(string message)
        : base(message)
    {

    }
}