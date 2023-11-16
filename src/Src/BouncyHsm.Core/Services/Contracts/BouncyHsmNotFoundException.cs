namespace BouncyHsm.Core.Services.Contracts;

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
}