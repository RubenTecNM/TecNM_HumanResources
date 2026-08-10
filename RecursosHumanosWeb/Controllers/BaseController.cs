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
                var userType = User.Claims.FirstOrDefault(c => c.Type == "UserType")?.Value;

                ViewData["IsAuthenticated"] = true;
                ViewData["IsAdministrator"] = userType == "1"; // Compara strings directamente
                ViewData["IsAutorizador"]   = userType == "2";
                ViewData["IsRH"]            = userType == "3";
                ViewData["UserName"]        = User.Identity.Name;
            }
            base.OnActionExecuting(context);
        }
    }
}
