using BouncyHsm.Core.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;

namespace BouncyHsm.Infrastructure.Filters;

public class HttpResponseErrorFilter : IActionFilter, IOrderedFilter
{
    private readonly ProblemDetailsFactory problemDetailsFactory;
    private readonly ILogger<HttpResponseErrorFilter> logger;

    public int Order
    {
        get => int.MaxValue - 10;
    }

    public HttpResponseErrorFilter(ProblemDetailsFactory problemDetailsFactory,
        ILogger<HttpResponseErrorFilter> logger)
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
        if (!context.ModelState.IsValid)
        {
            string errorSummary = this.BuildErrorSummary(context.ModelState);
            this.logger.LogError(context.Exception, "Model state validation error with on path {Method} {Path}: {ErrorSummary}.",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            errorSummary);
        }

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
        this.logger.LogError(context.Exception, "Error with traceId {traceId} on path {Method} {Path}: {ExceptionMessage}.",
            traceId,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            context.Exception.Message);
    }

    private string BuildErrorSummary(ModelStateDictionary modelState)
    {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<string, ModelStateEntry> state in modelState)
        {
            sb.AppendFormat("Property {0} with errors [", state.Key);
            bool isFirst = true;
            foreach (ModelError error in state.Value.Errors)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append("; ");
                }

                sb.Append(error.ErrorMessage);
            }
            sb.Append("]. ");
        }

        return sb.ToString();
    }
}
