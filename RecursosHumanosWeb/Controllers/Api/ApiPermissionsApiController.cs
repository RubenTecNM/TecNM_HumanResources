using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Controllers.Api
{
    [Route("api/ApiPermissions")]
    [ApiController]
    public class ApiPermisosApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;

        public ApiPermisosApiController(HumanResourcesContext context)
        {
            _context = context;
        }

        // GET: api/ApiPermissions
        // Este endpoint puede ser público o requerir una ApiKey maestra. 
        // Para consistencia y seguridad, requeriremos al menos permission de Leer sobre alguna tabla "Sistema" o similar,
        // pero como "ApiPermission" no es una tabla de negocio per se, podríamos dejarlo abierto o con un permission básico.
        // Asumiremos que requiere permission de Leer sobre "ApiPermission" si existiera en DatabaseTable, o simplemente "DatabaseTable".
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "DatabaseTable", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetApiPermisos()
        {
            var permissions = await _context.ApiPermissions
                .Where(p => p.Status == true)
                .Select(p => new
                {
                    p.Id,
                    p.Description
                })
                .ToListAsync();

            if (!permissions.Any()) return NotFound("No se encontraron permisos de API definidos.");

            return Ok(permissions);
        }
    }
}
