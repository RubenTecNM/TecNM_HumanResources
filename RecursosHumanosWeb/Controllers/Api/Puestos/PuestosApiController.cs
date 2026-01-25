using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Puestos;

namespace RecursosHumanosWeb.Controllers.Api.Puestos
{
    [Route("api/Puestos")]
    [ApiController]
    public class PuestosApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PuestosApiController> _logger;

        public PuestosApiController(RecursosHumanosContext context, IMapper mapper, ILogger<PuestosApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Puestos
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Puesto", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetPuestos()
        {
            var puestos = await _context.Puestos
                .Where(p => p.Estatus == true)
                .Select(p => new
                {
                    p.Id,
                    p.ClaveTabular,
                    p.Descripcion
                })
                .ToListAsync();

            if (!puestos.Any()) return NotFound("No se encontraron puestos activos.");

            return Ok(puestos);
        }

        // GET: api/Puestos/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Puesto", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetPuesto(int id)
        {
            var puesto = await _context.Puestos
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.ClaveTabular,
                    p.Descripcion,
                    p.Estatus,
                    p.FechaCreacion,
                    p.FechaModificacion,
                    CreadoPor = p.IdUsuarioCreaNavigation.Nombre,
                    ModificadoPor = p.IdUsuarioModificaNavigation.Nombre
                })
                .FirstOrDefaultAsync();

            if (puesto == null) return NotFound($"Puesto con ID {id} no encontrado.");

            return Ok(puesto);
        }

        // POST: api/Puestos
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "Puesto", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostPuesto([FromBody] PuestoCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var puesto = _mapper.Map<Puesto>(model);
            puesto.Estatus = true;
            puesto.FechaCreacion = DateTime.Now;
            puesto.FechaModificacion = DateTime.Now;
            puesto.IdUsuarioCrea = userId;
            puesto.IdUsuarioModifica = userId;

            try
            {
                _context.Puestos.Add(puesto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPuesto), new { id = puesto.Id }, new { puesto.Id, puesto.ClaveTabular, puesto.Descripcion });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear puesto vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Puestos/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Puesto", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutPuesto(int id, [FromBody] PuestoUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? puesto.IdUsuarioModifica;

            _mapper.Map(model, puesto);
            puesto.FechaModificacion = DateTime.Now;
            puesto.IdUsuarioModifica = userId;

            try
            {
                _context.Update(puesto);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar puesto {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // DELETE: api/Puestos/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Puesto", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeletePuesto(int id)
        {
            var puesto = await _context.Puestos.FindAsync(id);
            if (puesto == null) return NotFound();

            if (!puesto.Estatus) return BadRequest("El puesto ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? puesto.IdUsuarioModifica;

            puesto.Estatus = false;
            puesto.FechaModificacion = DateTime.Now;
            puesto.IdUsuarioModifica = userId;

            _context.Update(puesto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
