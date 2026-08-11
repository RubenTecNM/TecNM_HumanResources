using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models.ViewModels.Cutoffs;

namespace RecursosHumanosWeb.Controllers.Api.Cutoffs
{
    [Route("api/Cutoffs")]
    [ApiController]
    public class CutoffsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;

        public CutoffsApiController(HumanResourcesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Cutoffs
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        // public async Task<IActionResult> GetCutoffs() --> Se sugiere paginación para cutoffs, pero por ahora lista simple
        public async Task<IActionResult> GetCutoffs()
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
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetCutoff(int id)
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
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostCutoff([FromBody] CutoffApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var cutoff = _mapper.Map<Cutoff>(model);
            cutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Open; // Open
            cutoff.CreatedAt = DateTime.Now;
            cutoff.UpdatedAt = DateTime.Now;
            cutoff.CreatedByUserId = userId;
            cutoff.UpdatedByUserId = userId;

            _context.Cutoffs.Add(cutoff);
            // Nota: Aquí se podría validar solapamiento de fechas, etc.
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCutoff), new { id = cutoff.Id }, new { cutoff.Id, cutoff.StartDate, cutoff.EndDate });
        }

        // PUT: api/Cutoffs/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutCutoff(int id, [FromBody] CutoffApiUpdateViewModel model)
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
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeleteCutoff(int id)
        {
            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return NotFound();

            // Los cutoffs usualmente no se borran si tienen permissions, pero aquí hacemos soft delete básico
            if (cutoff.CutoffStatusTypeId == (int)CutoffStatusEnum.Closed) return BadRequest("El corte ya está cerrado/inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? cutoff.UpdatedByUserId;

            cutoff.CutoffStatusTypeId = (int)CutoffStatusEnum.Closed; // Cerrar/Inactivar
            cutoff.UpdatedAt = DateTime.Now;
            cutoff.UpdatedByUserId = userId;

            _context.Update(cutoff);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
