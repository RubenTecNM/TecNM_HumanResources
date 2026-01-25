using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.TipoPermisos;

namespace RecursosHumanosWeb.Controllers.Api.TipoPermisos
{
    [Route("api/TipoPermisos")]
    [ApiController]
    public class TipoPermisosApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TipoPermisosApiController> _logger;

        public TipoPermisosApiController(RecursosHumanosContext context, IMapper mapper, ILogger<TipoPermisosApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/TipoPermisos
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "TipoPermiso", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetTipoPermisos()
        {
            var tipoPermisos = await _context.TipoPermisos
                .Where(t => t.Estatus == true)
                .Select(t => new
                {
                    t.Id,
                    t.Descripcion
                })
                .ToListAsync();

            if (!tipoPermisos.Any()) return NotFound("No se encontraron tipos de permiso activos.");

            return Ok(tipoPermisos);
        }

        // GET: api/TipoPermisos/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "TipoPermiso", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetTipoPermiso(int id)
        {
            var tipoPermiso = await _context.TipoPermisos
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Descripcion,
                    t.Estatus,
                    t.FechaCreacion,
                    t.FechaModificacion,
                    CreadoPor = t.IdUsuarioCreaNavigation.Nombre,
                    ModificadoPor = t.IdUsuarioModificaNavigation.Nombre
                })
                .FirstOrDefaultAsync();

            if (tipoPermiso == null) return NotFound($"Tipo de permiso con ID {id} no encontrado.");

            return Ok(tipoPermiso);
        }

        // POST: api/TipoPermisos
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "TipoPermiso", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostTipoPermiso([FromBody] TipoPermisoCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var tipoPermiso = _mapper.Map<TipoPermiso>(model);
            tipoPermiso.Estatus = true;
            tipoPermiso.FechaCreacion = DateTime.Now;
            tipoPermiso.FechaModificacion = DateTime.Now;
            tipoPermiso.IdUsuarioCrea = userId;
            tipoPermiso.IdUsuarioModifica = userId;

            try
            {
                _context.TipoPermisos.Add(tipoPermiso);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTipoPermiso), new { id = tipoPermiso.Id }, new { tipoPermiso.Id, tipoPermiso.Descripcion });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear tipo de permiso vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/TipoPermisos/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "TipoPermiso", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutTipoPermiso(int id, [FromBody] TipoPermisoUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var tipoPermiso = await _context.TipoPermisos.FindAsync(id);
            if (tipoPermiso == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? tipoPermiso.IdUsuarioModifica;

            _mapper.Map(model, tipoPermiso);
            tipoPermiso.FechaModificacion = DateTime.Now;
            tipoPermiso.IdUsuarioModifica = userId;

            try
            {
                _context.Update(tipoPermiso);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar tipo de permiso {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // DELETE: api/TipoPermisos/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "TipoPermiso", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteTipoPermiso(int id)
        {
            var tipoPermiso = await _context.TipoPermisos.FindAsync(id);
            if (tipoPermiso == null) return NotFound();

            if (!tipoPermiso.Estatus) return BadRequest("El tipo de permiso ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? tipoPermiso.IdUsuarioModifica;

            tipoPermiso.Estatus = false;
            tipoPermiso.FechaModificacion = DateTime.Now;
            tipoPermiso.IdUsuarioModifica = userId;

            _context.Update(tipoPermiso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
