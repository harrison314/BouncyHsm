using BouncyHsm.Core.UseCases.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests;

internal static class AssertExtensions
{
    public static T AssertOkValue<T>(this DomainResult<T> result)
    {
        Assert.IsNotNull(result);
        if (result is DomainResult<T>.Ok ok)
        {
            return ok.Value;
        }

        Assert.Fail($"Result {result} is not OK domain result.");
        return default;
    }

    public static void AssertOk(this VoidDomainResult result)
    {
        Assert.IsInstanceOfType(result, typeof(VoidDomainResult.Ok));
    }

    public static void IsNotNullOrEmpty(this Assert _, string value, string? message = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            Assert.Fail(message ?? "Value can not by string or empty.");
        }
    }
}
