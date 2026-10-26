using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace HumanResourcesProduction.Filters;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment,
    ProblemDetailsFactory problemDetailsFactory) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Excepción no manejada: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (IsApiRequest(httpContext))
        {
            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                StatusCodes.Status500InternalServerError);

            if (environment.IsDevelopment())
            {
                problemDetails.Title = "Internal Server Error";
                problemDetails.Extensions["detail"] = exception.ToString();
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        httpContext.Response.ContentType = "text/plain; charset=utf-8";
        var message = "Ha ocurrido un error inesperado en el servidor. Por favor intente nuevamente más tarde.";
        if (environment.IsDevelopment())
        {
            message = $"Error: {exception.Message}";
        }

        await httpContext.Response.WriteAsync(message, cancellationToken);
        return true;
    }

    private static bool IsApiRequest(HttpContext httpContext)
    {
        return httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || httpContext.Request.Headers.Accept.ToString().Contains("application/json");
    }
}
