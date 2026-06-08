namespace BouncyHsm.Core.Services.Contracts;

public class BouncyHsmInvalidInputException : Exception
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
}