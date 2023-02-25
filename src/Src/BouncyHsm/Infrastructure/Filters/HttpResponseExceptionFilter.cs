using BouncyHsm.Core.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BouncyHsm.Infrastructure.Filters;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    private readonly ProblemDetailsFactory problemDetailsFactory;
    private readonly ILogger<HttpResponseExceptionFilter> logger;

    public int Order
    {
        get => int.MaxValue - 10;
    }

    public HttpResponseExceptionFilter(ProblemDetailsFactory problemDetailsFactory,
        ILogger<HttpResponseExceptionFilter> logger)
    {
        this.problemDetailsFactory = problemDetailsFactory;
        this.logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        //NOP
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception == null)
        {
            return;
        }

        ProblemDetails details;
        if (context.Exception is BouncyHsmInvalidInputException inputException)
        {
            details = this.problemDetailsFactory.CreateProblemDetails(context.HttpContext,
                statusCode: 400,
                detail: inputException.Message);
        }
        else
        if (context.Exception is BouncyHsmException bouncyHsmException)
        {
            details = this.problemDetailsFactory.CreateProblemDetails(context.HttpContext,
                statusCode: 500,
                detail: bouncyHsmException.Message);
        }
        else
        {
            details = this.problemDetailsFactory.CreateProblemDetails(context.HttpContext,
                statusCode: 500,
                detail: "An unexpected error occurred.");
        }

        object? traceId = string.Empty;
        details.Extensions?.TryGetValue("traceId", out traceId);

        context.Result = new ObjectResult(details)
        {
            StatusCode = details.Status ?? 500
        };

        context.ExceptionHandled = true;
        this.logger.LogError(context.Exception, "Error with traceId {traceId} on path {Path}: {ExceptionMessage}.",
            traceId,
            context.HttpContext.Request.Path,
            context.Exception.Message);
    }
}
