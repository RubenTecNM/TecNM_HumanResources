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
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AreasApiController> _logger;

        public AreasApiController(RecursosHumanosContext context, IMapper mapper, ILogger<AreasApiController> logger)
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
                .Where(a => a.Estatus == true)
                .Select(a => new
                {
                    a.Id,
                    a.Descripcion
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
                .Where(a => a.Id == id && a.Estatus == true)
                .Select(a => new
                {
                    a.Id,
                    a.Descripcion,
                    a.Estatus,
                    a.FechaCreacion,
                    a.FechaModificacion,
                    CreadoPor = a.IdUsuarioCreaNavigation.Nombre,
                    ModificadoPor = a.IdUsuarioModificaNavigation.Nombre
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
            area.Estatus = true;
            area.FechaCreacion = DateTime.Now;
            area.FechaModificacion = DateTime.Now;
            area.IdUsuarioCrea = userId;
            area.IdUsuarioModifica = userId;

            try
            {
                _context.Areas.Add(area);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetArea), new { id = area.Id }, new { area.Id, area.Descripcion });
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

            var userId = HttpContext.Items["ApiUserId"] as int? ?? area.IdUsuarioModifica;

            _mapper.Map(model, area);
            area.FechaModificacion = DateTime.Now;
            area.IdUsuarioModifica = userId;

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

            if (!area.Estatus) return BadRequest("El área ya está inactiva.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? area.IdUsuarioModifica;

            area.Estatus = false;
            area.FechaModificacion = DateTime.Now;
            area.IdUsuarioModifica = userId; // Soft delete attribution

            _context.Update(area);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
