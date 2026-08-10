using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Cutoffs;

namespace RecursosHumanosWeb.Controllers.Api.Cutoffs
{
    [Route("api/Cutoffs")]
    [ApiController]
    public class CortesApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;

        public CortesApiController(HumanResourcesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Cutoffs
        [HttpGet]
        [ApiKeyAuthorize(TablaObjetivo = "Cutoff", PermisoRequerido = "Leer")]
        // public async Task<IActionResult> GetCortes() --> Se sugiere paginación para cutoffs, pero por ahora lista simple
        public async Task<IActionResult> GetCortes()
        {
            var cutoffs = await _context.Cutoffs
                .OrderByDescending(c => c.Id)
                .Select(c => new
                {
                    c.Id,
                    c.StartDate,
                    c.EndDate,
                    c.CutoffStatusTypeId
                })
                .ToListAsync();

            if (!cutoffs.Any()) return NotFound("No se encontraron cutoffs.");

            return Ok(cutoffs);
        }

        // GET: api/Cutoffs/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Cutoff", PermisoRequerido = "Leer")]
        public async Task<IActionResult> GetCorte(int id)
        {
            var cutoff = await _context.Cutoffs
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.StartDate,
                    c.EndDate,
                    c.CutoffStatusTypeId,
                    c.CreatedAt,
                    c.UpdatedAt,
                    CreatedBy = c.CreatedByUser.Name,
                    UpdatedBy = c.UpdatedByUser != null ? c.UpdatedByUser.Name : "N/A"
                })
                .FirstOrDefaultAsync();

            if (cutoff == null) return NotFound($"Cutoff con ID {id} no encontrado.");

            return Ok(cutoff);
        }

        // POST: api/Cutoffs
        [HttpPost]
        [ApiKeyAuthorize(TablaObjetivo = "Cutoff", PermisoRequerido = "Escribir")]
        public async Task<IActionResult> PostCorte([FromBody] CutoffApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var cutoff = _mapper.Map<Cutoff>(model);
            cutoff.CutoffStatusTypeId = 1; // Open
            cutoff.CreatedAt = DateTime.Now;
            cutoff.UpdatedAt = DateTime.Now;
            cutoff.CreatedByUserId = userId;
            cutoff.UpdatedByUserId = userId;

            _context.Cutoffs.Add(cutoff);
            // Nota: Aquí se podría validar solapamiento de fechas, etc.
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCorte), new { id = cutoff.Id }, new { cutoff.Id, cutoff.StartDate, cutoff.EndDate });
        }

        // PUT: api/Cutoffs/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Cutoff", PermisoRequerido = "Actualizar")]
        public async Task<IActionResult> PutCorte(int id, [FromBody] CutoffApiUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? cutoff.UpdatedByUserId;

            _mapper.Map(model, cutoff);
            cutoff.UpdatedAt = DateTime.Now;
            cutoff.UpdatedByUserId = userId;

            _context.Update(cutoff);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cutoffs/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TablaObjetivo = "Cutoff", PermisoRequerido = "Eliminar")]
        public async Task<IActionResult> DeleteCorte(int id)
        {
            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return NotFound();

            // Los cutoffs usualmente no se borran si tienen permissions, pero aquí hacemos soft delete básico
            if (cutoff.CutoffStatusTypeId == 2) return BadRequest("El corte ya está cerrado/inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? cutoff.UpdatedByUserId;

            cutoff.CutoffStatusTypeId = 2; // Cerrar/Inactivar
            cutoff.UpdatedAt = DateTime.Now;
            cutoff.UpdatedByUserId = userId;

            _context.Update(cutoff);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
