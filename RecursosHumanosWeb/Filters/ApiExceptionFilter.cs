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
            // Solo actuar si es un controlador de API (o si lo preferimos para todo el sitio, pero usualmente es para API)
            // Se puede verificar el path o si el controlador tiene [ApiController]
            
            var exception = context.Exception;
            var inner = exception.InnerException?.Message ?? "No inner exception";

            _logger.LogError(exception, "Error no manejado en la API. Detalle: {Inner}", inner);

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
