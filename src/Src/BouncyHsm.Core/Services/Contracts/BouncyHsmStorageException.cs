namespace BouncyHsm.Core.Services.Contracts;

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
}