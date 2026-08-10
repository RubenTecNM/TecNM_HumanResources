// --- EN ApiKeyAuthorizeAttribute.cs ---

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string? TargetTable { get; set; }
        public string? RequiredPermission { get; set; }

        private const string HeaderApiKeyName = "X-API-KEY";
        private const string QueryStringApiKeyName = "api_key"; // Name del parámetro en la URL

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<HumanResourcesContext>();
            string? extractedApiKey = null;

            // 1. INTENTAR OBTENER LA CLAVE DEL ENCABEZADO (MÉTODO PREFERIDO Y SEGURO)
            if (context.HttpContext.Request.Headers.TryGetValue(HeaderApiKeyName, out var headerApiKey))
            {
                extractedApiKey = headerApiKey.ToString();
            }
            // 2. SI NO ESTÁ EN EL HEADER, INTENTAR OBTENERLA DEL QUERY STRING (MÉTODO DE PRUEBA)
            else if (context.HttpContext.Request.Query.ContainsKey(QueryStringApiKeyName))
            {
                extractedApiKey = context.HttpContext.Request.Query[QueryStringApiKeyName].ToString();
            }

            // 3. SI NO SE ENCONTRÓ EN NINGÚN LUGAR, NEGAR EL ACCESO
            if (string.IsNullOrEmpty(extractedApiKey))
            {
                // Usamos UnauthorizedObjectResult para asegurar una respuesta API limpia (401)
                context.Result = new UnauthorizedObjectResult(new
                {
                    error = "Unauthorized",
                    message = $"Se requiere el encabezado '{HeaderApiKeyName}' o el parámetro de URL '{QueryStringApiKeyName}'."
                });
                return;
            }

            // 4. BÚSQUEDA Y VALIDACIÓN EN LA BASE DE DATOS (Misma lógica que antes)
            var apiKeyEntity = await dbContext.ApiKeys
                // Incluimos las relaciones necesarias para la validación de permissions
                .Include(k => k.ApiKeyTablePermissions)
                    .ThenInclude(pkt => pkt.Table)
                .Include(k => k.ApiKeyTablePermissions)
                    .ThenInclude(pkt => pkt.ApiPermission)
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.Key == extractedApiKey && k.Status == true);

            if (apiKeyEntity == null)
            {
                context.Result = new ObjectResult(new { error = "Forbidden", message = "API Key no válida o inactiva." }) { StatusCode = 403 };
                return;
            }

            // 5. VALIDACIÓN DE PERMISOS GRANULARES (Sin cambios)
            if (!string.IsNullOrEmpty(TargetTable) && !string.IsNullOrEmpty(RequiredPermission))
            {
                bool hasPermission = apiKeyEntity.ApiKeyTablePermissions.Any(pkt =>
                    pkt.Status == true &&
                    pkt.Table.Description.Equals(TargetTable, StringComparison.OrdinalIgnoreCase) &&
                    pkt.ApiPermission.Description.Equals(RequiredPermission, StringComparison.OrdinalIgnoreCase)
                );

                if (!hasPermission)
                {
                    context.Result = new ObjectResult(new { error = "Forbidden", message = $"La API Key no tiene permission '{RequiredPermission}' para la tabla '{TargetTable}'." }) { StatusCode = 403 };
                    return;
                }
            }

            // 6. ÉXITO: Continuar
            context.HttpContext.Items["ApiKeyId"] = apiKeyEntity.Id;
            context.HttpContext.Items["ApiUserId"] = apiKeyEntity.CreatedByUserId;
        }
    }
}