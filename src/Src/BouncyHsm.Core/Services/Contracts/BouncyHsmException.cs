namespace BouncyHsm.Core.Services.Contracts;

public class BouncyHsmException : Exception
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
}
