using System.Runtime.Serialization;

namespace BouncyHsm.Core.Services.Contracts;

[Serializable]
public class BouncyHsmStorageException : BouncyHsmException
{
    public BouncyHsmStorageException()
    {
    }

    public BouncyHsmStorageException(string? message) : base(message)
    {
    }

    public BouncyHsmStorageException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected BouncyHsmStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}