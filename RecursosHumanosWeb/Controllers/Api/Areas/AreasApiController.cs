using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Areas;
using RecursosHumanosWeb.Models.DTOs; // Assuming generic DTOs might be here, if not we use anonymous or specific VMs

namespace RecursosHumanosWeb.Controllers.Api.Areas
{
    [Route("api/Areas")]
    [ApiController]
    public class AreasApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AreasApiController> _logger;

        public AreasApiController(HumanResourcesContext context, IMapper mapper, ILogger<AreasApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Areas
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Area", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _context.Areas
                .Where(a => a.Status == true)
                .Select(a => new
                {
                    a.Id,
                    a.Description
                })
                .ToListAsync();

            if (!areas.Any()) return NotFound("No se encontraron áreas activas.");

            return Ok(areas);
        }

        // GET: api/Areas/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Area", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetArea(int id)
        {
            var area = await _context.Areas
                .Where(a => a.Id == id && a.Status == true)
                .Select(a => new
                {
                    a.Id,
                    a.Description,
                    a.Status,
                    a.CreatedAt,
                    a.UpdatedAt,
                    CreatedBy = a.CreatedByUser.Name,
                    UpdatedBy = a.UpdatedByUser.Name
                })
                .FirstOrDefaultAsync();

            if (area == null) return NotFound($"Área con ID {id} no encontrada.");

            return Ok(area);
        }

        // POST: api/Areas
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "Area", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostArea([FromBody] AreaCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var area = _mapper.Map<Area>(model);
            area.Status = true;
            area.CreatedAt = DateTime.Now;
            area.UpdatedAt = DateTime.Now;
            area.CreatedByUserId = userId;
            area.UpdatedByUserId = userId;

            try
            {
                _context.Areas.Add(area);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetArea), new { id = area.Id }, new { area.Id, area.Description });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear área vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Areas/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Area", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutArea(int id, [FromBody] AreaUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var area = await _context.Areas.FindAsync(id);
            if (area == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? area.UpdatedByUserId;

            _mapper.Map(model, area);
            area.UpdatedAt = DateTime.Now;
            area.UpdatedByUserId = userId;

            try
            {
                _context.Update(area);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar área {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // DELETE: api/Areas/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Area", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteArea(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return NotFound();

            if (!area.Status) return BadRequest("El área ya está inactiva.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? area.UpdatedByUserId;

            area.Status = false;
            area.UpdatedAt = DateTime.Now;
            area.UpdatedByUserId = userId; // Soft delete attribution

            _context.Update(area);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
