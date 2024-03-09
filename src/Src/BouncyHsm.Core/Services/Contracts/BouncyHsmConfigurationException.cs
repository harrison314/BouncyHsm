namespace BouncyHsm.Core.Services.Contracts;

public class BouncyHsmConfigurationException : BouncyHsmException
{
    public BouncyHsmConfigurationException()
    {
    }

    public BouncyHsmConfigurationException(string? message)
        : base(message)
    {
    }

    public BouncyHsmConfigurationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
