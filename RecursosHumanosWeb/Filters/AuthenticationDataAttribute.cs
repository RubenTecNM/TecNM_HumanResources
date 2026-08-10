using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace RecursosHumanosWeb.Filters
{
    /// <summary>
    /// Filtro de acción que inyecta datos de autenticación y roles del user
    /// en el ViewData de cada acción MVC. Registrado globalmente en Program.cs.
    /// </summary>
    public class AuthenticationDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                controller.ViewData["IsAuthenticated"] = context.HttpContext.User.Identity?.IsAuthenticated;
                controller.ViewData["IsAdministrador"] = context.HttpContext.User.HasClaim("UserType", "1");
                controller.ViewData["IsAutorizador"]   = context.HttpContext.User.HasClaim("UserType", "2");
                controller.ViewData["IsRH"]            = context.HttpContext.User.HasClaim("UserType", "3");
                controller.ViewData["IsEmpleado"]      = context.HttpContext.User.HasClaim("UserType", "4");
                controller.ViewData["UserName"]        = context.HttpContext.User.Identity?.Name;
                controller.ViewData["IdUsuario"]       = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            base.OnActionExecuting(context);
        }
    }
}
