using BouncyHsm.Core.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Infrastructure;

internal sealed class HttpResponseErrorMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<HttpResponseErrorMiddleware> logger;

    public HttpResponseErrorMiddleware(RequestDelegate next, ILogger<HttpResponseErrorMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            int statusCode = exception switch
            {
                BouncyHsmInvalidInputException => StatusCodes.Status400BadRequest,
                BouncyHsmException => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

            string? detail = exception switch
            {
                BouncyHsmInvalidInputException or BouncyHsmException => exception.Message,
                _ => "An unexpected error occurred."
            };

            ProblemDetails problemDetails = new()
            {
                Status = statusCode,
                Detail = detail
            };
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            this.logger.LogError(exception,
                "Error with traceId {traceId} on path {Method} {Path}: {ExceptionMessage}.",
                context.TraceIdentifier,
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            context.Response.Clear();
            await Results.Problem(problemDetails).ExecuteAsync(context);
        }
    }
}
