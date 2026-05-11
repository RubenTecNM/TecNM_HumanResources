using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Cortes;

namespace RecursosHumanosWeb.Controllers.Api.Cortes
{
    [Route("api/Cortes")]
    [ApiController]
    public class CortesApiController : ControllerBase
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;

        public CortesApiController(RecursosHumanosContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Cortes
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Corte", PermisoRequerido = "Leer")]
        // public async Task<IActionResult> GetCortes() --> Se sugiere paginación para cortes, pero por ahora lista simple
        public async Task<IActionResult> GetCortes()
        {
            var cortes = await _context.Cortes
                .OrderByDescending(c => c.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Inicia,
                    c.Termina,
                    c.IdEstadoCorte
                })
                .ToListAsync();

            if (!cortes.Any()) return NotFound("No se encontraron cortes.");

            return Ok(cortes);
        }

        // GET: api/Cortes/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Corte", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetCorte(int id)
        {
            var corte = await _context.Cortes
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Inicia,
                    c.Termina,
                    c.IdEstadoCorte,
                    c.FechaCreacion,
                    c.FechaModificacion,
                    CreadoPor = c.IdUsuarioCreaNavigation.Nombre,
                    ModificadoPor = c.IdUsuarioModificaNavigation != null ? c.IdUsuarioModificaNavigation.Nombre : "N/A"
                })
                .FirstOrDefaultAsync();

            if (corte == null) return NotFound($"Corte con ID {id} no encontrado.");

            return Ok(corte);
        }

        // POST: api/Cortes
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "Corte", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostCorte([FromBody] CorteApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var corte = _mapper.Map<Corte>(model);
            corte.IdEstadoCorte = 1; // Abierto
            corte.FechaCreacion = DateTime.Now;
            corte.FechaModificacion = DateTime.Now;
            corte.IdUsuarioCrea = userId;
            corte.IdUsuarioModifica = userId;

            _context.Cortes.Add(corte);
            // Nota: Aquí se podría validar solapamiento de fechas, etc.
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCorte), new { id = corte.Id }, new { corte.Id, corte.Inicia, corte.Termina });
        }

        // PUT: api/Cortes/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Corte", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutCorte(int id, [FromBody] CorteApiUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var corte = await _context.Cortes.FindAsync(id);
            if (corte == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? corte.IdUsuarioModifica;

            _mapper.Map(model, corte);
            corte.FechaModificacion = DateTime.Now;
            corte.IdUsuarioModifica = userId;

            _context.Update(corte);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cortes/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Corte", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteCorte(int id)
        {
            var corte = await _context.Cortes.FindAsync(id);
            if (corte == null) return NotFound();

            // Los cortes usualmente no se borran si tienen permisos, pero aquí hacemos soft delete básico
            if (corte.IdEstadoCorte == 2) return BadRequest("El corte ya está cerrado/inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? corte.IdUsuarioModifica;

            corte.IdEstadoCorte = 2; // Cerrar/Inactivar
            corte.FechaModificacion = DateTime.Now;
            corte.IdUsuarioModifica = userId;

            _context.Update(corte);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
