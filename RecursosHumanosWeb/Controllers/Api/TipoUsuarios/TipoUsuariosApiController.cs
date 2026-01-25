using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.TipoUsuarios;

namespace RecursosHumanosWeb.Controllers.Api.TipoUsuarios
{
    [Route("api/TipoUsuarios")]
    [ApiController]
    public class TipoUsuariosApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TipoUsuariosApiController> _logger;

        public TipoUsuariosApiController(RecursosHumanosContext context, IMapper mapper, ILogger<TipoUsuariosApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/TipoUsuarios
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "TipoUsuario", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetTipoUsuarios()
        {
            var tipoUsuarios = await _context.TipoUsuarios
                .Where(t => t.Estatus == true)
                .Select(t => new
                {
                    t.Id,
                    t.Descripcion
                })
                .ToListAsync();

            if (!tipoUsuarios.Any()) return NotFound("No se encontraron tipos de usuario activos.");

            return Ok(tipoUsuarios);
        }

        // GET: api/TipoUsuarios/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "TipoUsuario", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetTipoUsuario(int id)
        {
            var tipoUsuario = await _context.TipoUsuarios
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

            if (tipoUsuario == null) return NotFound($"Tipo de usuario con ID {id} no encontrado.");

            return Ok(tipoUsuario);
        }

        // POST: api/TipoUsuarios
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "TipoUsuario", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostTipoUsuario([FromBody] TipoUsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var tipoUsuario = _mapper.Map<TipoUsuario>(model);
            tipoUsuario.Estatus = true;
            tipoUsuario.FechaCreacion = DateTime.Now;
            tipoUsuario.FechaModificacion = DateTime.Now;
            tipoUsuario.IdUsuarioCrea = userId;
            tipoUsuario.IdUsuarioModifica = userId;

            try
            {
                _context.TipoUsuarios.Add(tipoUsuario);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTipoUsuario), new { id = tipoUsuario.Id }, new { tipoUsuario.Id, tipoUsuario.Descripcion });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear tipo de usuario vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/TipoUsuarios/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "TipoUsuario", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutTipoUsuario(int id, [FromBody] TipoUsuarioUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var tipoUsuario = await _context.TipoUsuarios.FindAsync(id);
            if (tipoUsuario == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? tipoUsuario.IdUsuarioModifica;

            _mapper.Map(model, tipoUsuario);
            tipoUsuario.FechaModificacion = DateTime.Now;
            tipoUsuario.IdUsuarioModifica = userId;

            try
            {
                _context.Update(tipoUsuario);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar tipo de usuario {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // DELETE: api/TipoUsuarios/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "TipoUsuario", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteTipoUsuario(int id)
        {
            var tipoUsuario = await _context.TipoUsuarios.FindAsync(id);
            if (tipoUsuario == null) return NotFound();

            if (!tipoUsuario.Estatus) return BadRequest("El tipo de usuario ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? tipoUsuario.IdUsuarioModifica;

            tipoUsuario.Estatus = false;
            tipoUsuario.FechaModificacion = DateTime.Now;
            tipoUsuario.IdUsuarioModifica = userId;

            _context.Update(tipoUsuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
