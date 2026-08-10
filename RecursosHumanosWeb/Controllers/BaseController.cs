using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RecursosHumanosWeb.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Buscamos el valor de forma segura
                var tipo = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;

                ViewData["IsAuthenticated"] = true;
                ViewData["IsAdministrador"] = tipo == "1"; // Compara strings directamente
                ViewData["IsAutorizador"]   = tipo == "2";
                ViewData["IsRH"]            = tipo == "3";
                ViewData["UserName"]        = User.Identity.Name;
            }
            base.OnActionExecuting(context);
        }
    }
}
