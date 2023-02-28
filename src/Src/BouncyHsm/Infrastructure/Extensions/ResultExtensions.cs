
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Core.Infrastructure.Extensions;

internal static class DomainResultExtensions
{
    public static IActionResult ToActionResult(this VoidDomainResult domainResult)
    {
        return domainResult.Match<IActionResult>(ok => new OkResult(),
            notFound => ProblemNotFound(),
            invalidInput => ProblemBadRequest(invalidInput.Message));
    }

    public static IActionResult ToActionResult<T>(this DomainResult<T> domainResult)
    {
        return domainResult.Match<IActionResult>(ok => new ObjectResult(ok.Value),
            notFound => ProblemNotFound(),
            invalidInput => ProblemBadRequest(invalidInput.Message));
    }

    public static DomainResult<TResult> MapOk<T, TResult>(this DomainResult<T> domainResult, Func<T, TResult> mapper)
    {
        return domainResult.Match<DomainResult<TResult>>(ok => new DomainResult<TResult>.Ok(mapper(ok.Value)),
            notFound => new DomainResult<TResult>.NotFound(),
            invalidInput => new DomainResult<TResult>.InvalidInput(invalidInput.Message));
    }

    public static VoidDomainResult MapOkToVoid<T>(this DomainResult<T> domainResult)
    {
        return domainResult.Match<VoidDomainResult>(ok => new VoidDomainResult.Ok(),
            notFound => new VoidDomainResult.NotFound(),
            invalidInput => new VoidDomainResult.InvalidInput(invalidInput.Message));
    }

    private static IActionResult ProblemNotFound()
    {
        ProblemDetails problemDetails = new ProblemDetails()
        {
            Status = 404,
            Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
            Title = "Not Found",
            Detail = null
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = 404
        };
    }

    private static IActionResult ProblemBadRequest(string? description)
    {
        ProblemDetails problemDetails = new ProblemDetails()
        {
            Status = 400,
            Type = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Detail = description
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = 400
        };
    }
}