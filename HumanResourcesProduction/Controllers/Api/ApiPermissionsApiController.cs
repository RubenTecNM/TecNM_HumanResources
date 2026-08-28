using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;

namespace HumanResourcesProduction.Controllers.Api
{
    [Route("api/ApiPermissions")]
    [ApiController]
    public class ApiPermissionsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;

        public ApiPermissionsApiController(HumanResourcesContext context)
        {
            _context = context;
        }

        // GET: api/ApiPermissions
        // Este endpoint puede ser público o requerir una ApiKey maestra. 
        // Para consistencia y seguridad, requeriremos al menos permission de Leer sobre alguna tabla "Sistema" o similar,
        // pero como "ApiPermission" no es una tabla de negocio per se, podríamos dejarlo abierto o con un permission básico.
        // Asumiremos que requiere permission de Leer sobre "ApiPermission" si existiera en DatabaseTable, o simplemente "DatabaseTable".
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "DatabaseTable", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetApiPermissions()
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
