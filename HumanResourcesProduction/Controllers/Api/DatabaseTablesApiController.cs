using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResourcesProduction.Controllers.Api
{
    [Route("api/DatabaseTables")]
    [ApiController]
    public class TablasApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;

        public TablasApiController(HumanResourcesContext context)
        {
            _context = context;
        }

        // -----------------------------------------------------------------
        // GET: api/tablas
        // Permission requerido: Leer sobre la tabla "DatabaseTable"
        // -----------------------------------------------------------------
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "DatabaseTable", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetTablas()
        {
            // Solo devolvemos las tablas activas y campos relevantes para la API
            var tablas = await _context.DatabaseTables
                .Where(t => t.Status == true)
                .Select(t => new
                {
                    t.Id,
                    t.Description,
                    t.CreatedAt
                })
                .ToListAsync();

            if (!tablas.Any())
            {
                return NotFound("No se encontraron tablas activas.");
            }

            return Ok(tablas);
        }

        // -----------------------------------------------------------------
        // GET: api/tablas/{id}
        // Permission requerido: Leer sobre la tabla "DatabaseTable"
        // -----------------------------------------------------------------
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "DatabaseTable", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetTabla(int id)
        {
            var tabla = await _context.DatabaseTables
                .Where(t => t.Id == id && t.Status == true)
                .Select(t => new
                {
                    t.Id,
                    t.Description,
                    t.CreatedAt,
                    t.UpdatedAt,
                    // Incluir users de creación/modificación si es necesario
                    CreatedBy = t.CreatedByUser.Name,
                    UpdatedBy = t.UpdatedByUser.Name
                })
                .FirstOrDefaultAsync();

            if (tabla == null)
            {
                return NotFound($"DatabaseTable con ID {id} no encontrada o inactiva.");
            }

            return Ok(tabla);
        }

        // NOTA: No incluimos POST, PUT o DELETE a menos que sea necesario 
        // gestionar las tablas directamente desde la API, lo cual no es lo habitual.
    }
}