using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.PermissionTypes;

namespace RecursosHumanosWeb.Controllers.Api.PermissionTypes
{
    [Route("api/PermissionTypes")]
    [ApiController]
    public class TipoPermisosApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TipoPermisosApiController> _logger;

        public TipoPermisosApiController(HumanResourcesContext context, IMapper mapper, ILogger<TipoPermisosApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/PermissionTypes
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "PermissionType", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetTipoPermisos()
        {
            var permissionTypes = await _context.PermissionTypes
                .Where(t => t.Status == true)
                .Select(t => new
                {
                    t.Id,
                    t.Description
                })
                .ToListAsync();

            if (!permissionTypes.Any()) return NotFound("No se encontraron tipos de permiso activos.");

            return Ok(permissionTypes);
        }

        // GET: api/PermissionTypes/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "PermissionType", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetTipoPermiso(int id)
        {
            var permissionType = await _context.PermissionTypes
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Description,
                    t.Status,
                    t.CreatedAt,
                    t.UpdatedAt,
                    CreatedBy = t.CreatedByUser.Name,
                    UpdatedBy = t.UpdatedByUser.Name
                })
                .FirstOrDefaultAsync();

            if (permissionType == null) return NotFound($"Tipo de permiso con ID {id} no encontrado.");

            return Ok(permissionType);
        }

        // POST: api/PermissionTypes
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "PermissionType", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostTipoPermiso([FromBody] PermissionTypeCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var permissionType = _mapper.Map<PermissionType>(model);
            permissionType.Status = true;
            permissionType.CreatedAt = DateTime.Now;
            permissionType.UpdatedAt = DateTime.Now;
            permissionType.CreatedByUserId = userId;
            permissionType.UpdatedByUserId = userId;

            try
            {
                _context.PermissionTypes.Add(permissionType);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTipoPermiso), new { id = permissionType.Id }, new { permissionType.Id, permissionType.Description });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear tipo de permission vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/PermissionTypes/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "PermissionType", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutTipoPermiso(int id, [FromBody] PermissionTypeUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var permissionType = await _context.PermissionTypes.FindAsync(id);
            if (permissionType == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? permissionType.UpdatedByUserId;

            _mapper.Map(model, permissionType);
            permissionType.UpdatedAt = DateTime.Now;
            permissionType.UpdatedByUserId = userId;

            try
            {
                _context.Update(permissionType);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar tipo de permission {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // DELETE: api/PermissionTypes/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "PermissionType", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteTipoPermiso(int id)
        {
            var permissionType = await _context.PermissionTypes.FindAsync(id);
            if (permissionType == null) return NotFound();

            if (!permissionType.Status) return BadRequest("El tipo de permiso ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? permissionType.UpdatedByUserId;

            permissionType.Status = false;
            permissionType.UpdatedAt = DateTime.Now;
            permissionType.UpdatedByUserId = userId;

            _context.Update(permissionType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
