using System.Runtime.Serialization;

namespace BouncyHsm.Core.Services.Contracts;

[Serializable]
public class BouncyHsmInvalidInputException : ApplicationException
{
    public BouncyHsmInvalidInputException()
    {
    }

    public BouncyHsmInvalidInputException(string? message)
        : base(message)
    {
    }

    public BouncyHsmInvalidInputException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected BouncyHsmInvalidInputException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}