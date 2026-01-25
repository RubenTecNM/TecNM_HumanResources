using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Controllers.Api
{
    [Route("api/ApiPermisos")]
    [ApiController]
    public class ApiPermisosApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;

        public ApiPermisosApiController(RecursosHumanosContext context)
        {
            _context = context;
        }

        // GET: api/ApiPermisos
        // Este endpoint puede ser público o requerir una ApiKey maestra. 
        // Para consistencia y seguridad, requeriremos al menos permiso de Leer sobre alguna tabla "Sistema" o similar,
        // pero como "ApiPermiso" no es una tabla de negocio per se, podríamos dejarlo abierto o con un permiso básico.
        // Asumiremos que requiere permiso de Leer sobre "ApiPermiso" si existiera en Tabla, o simplemente "Tabla".
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Tabla", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetApiPermisos()
        {
            var permisos = await _context.ApiPermisos
                .Where(p => p.Estatus == true)
                .Select(p => new
                {
                    p.Id,
                    p.Descripcion
                })
                .ToListAsync();

            if (!permisos.Any()) return NotFound("No se encontraron permisos de API definidos.");

            return Ok(permisos);
        }
    }
}
