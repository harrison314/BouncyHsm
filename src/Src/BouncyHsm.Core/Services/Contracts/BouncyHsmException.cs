using System.Runtime.Serialization;

namespace BouncyHsm.Core.Services.Contracts;

[Serializable]
public class BouncyHsmException : ApplicationException
{
    public BouncyHsmException()
    {
    }

    public BouncyHsmException(string? message)
        : base(message)
    {
    }

    public BouncyHsmException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected BouncyHsmException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
