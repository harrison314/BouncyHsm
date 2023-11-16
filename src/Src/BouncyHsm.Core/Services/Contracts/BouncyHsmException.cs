namespace BouncyHsm.Core.Services.Contracts;

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
}
