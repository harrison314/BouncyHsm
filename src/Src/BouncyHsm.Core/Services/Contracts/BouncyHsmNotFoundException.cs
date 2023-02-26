using System.Runtime.Serialization;

namespace BouncyHsm.Core.Services.Contracts;

[Serializable]
public class BouncyHsmNotFoundException : BouncyHsmException
{
    public BouncyHsmNotFoundException()
    {
    }

    public BouncyHsmNotFoundException(string? message) 
        : base(message)
    {
    }

    public BouncyHsmNotFoundException(string? message, Exception? innerException) 
        : base(message, innerException)
    {
    }

    protected BouncyHsmNotFoundException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}