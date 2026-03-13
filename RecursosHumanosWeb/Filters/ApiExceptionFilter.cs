using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace RecursosHumanosWeb.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            // Solo actuar si es una solicitud AJAX o si explícitamente se espera JSON
            var isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var acceptsJson = context.HttpContext.Request.Headers["Accept"].ToString().Contains("application/json");

            if (!isAjax && !acceptsJson)
            {
                // Permitir que el flujo siga al ErrorHandler estándar para vistas MVC
                return;
            }
            
            var exception = context.Exception;
            var inner = exception.InnerException?.Message ?? "No inner exception";

            _logger.LogError(exception, "Error no manejado en llamada asíncrona/API. Detalle: {Inner}", inner);

            var response = new
            {
                error = "Internal Server Error",
                message = exception.Message,
                detail = inner
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.ExceptionHandled = true;
        }
    }
}
