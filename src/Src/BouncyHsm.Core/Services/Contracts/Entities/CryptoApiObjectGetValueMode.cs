namespace BouncyHsm.Core.Services.Contracts.Entities;

public enum CryptoApiObjectGetValueMode
{
    /// <summary>
    /// Default behoviar.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Skip computing or updating value, returns old value for size detection.
    /// </summary>
    SkipComputing = 1
}