using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels.Positions;

namespace HumanResourcesProduction.Features.Positions
{
    [Route("api/Positions")]
    [ApiController]
    public class PuestosApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PuestosApiController> _logger;

        public PuestosApiController(HumanResourcesContext context, IMapper mapper, ILogger<PuestosApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Positions
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "Position", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPositions([FromQuery] bool? all = false)
        {
            var query = _context.Positions.AsQueryable();
            if (all != true)
            {
                query = query.Where(p => p.Status == true);
            }

            var positions = await query
                .OrderBy(p => p.Description)
                .Select(p => new
                {
                    p.Id,
                    p.TabularCode,
                    p.Description,
                    p.Status,
                    UserCount = _context.Users.Count(u => u.PositionId == p.Id && u.Status)
                })
                .ToListAsync();

            return Ok(positions);
        }

        // GET: api/Positions/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "Position", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPosition(int id)
        {
            var position = await _context.Positions
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.TabularCode,
                    p.Description,
                    p.Status,
                    p.CreatedAt,
                    p.UpdatedAt,
                    CreatedBy = p.CreatedByUser.Name,
                    UpdatedBy = p.UpdatedByUser.Name
                })
                .FirstOrDefaultAsync();

            if (position == null) return NotFound($"Position con ID {id} no encontrado.");

            return Ok(position);
        }

        // POST: api/Positions
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "Position", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostPosition([FromBody] PositionCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var position = _mapper.Map<Position>(model);
            position.Status = true;
            position.CreatedAt = DateTime.Now;
            position.UpdatedAt = DateTime.Now;
            position.CreatedByUserId = userId;
            position.UpdatedByUserId = userId;

            try
            {
                _context.Positions.Add(position);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPosition), new { id = position.Id }, new { position.Id, position.TabularCode, position.Description });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear position vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Positions/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "Position", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutPosition(int id, [FromBody] PositionUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var position = await _context.Positions.FindAsync(id);
            if (position == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? position.UpdatedByUserId;

            _mapper.Map(model, position);
            position.UpdatedAt = DateTime.Now;
            position.UpdatedByUserId = userId;

            try
            {
                _context.Update(position);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar position {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Positions/5/deactivate  (borrado LÓGICO: no existe DELETE físico)
        [HttpPut("{id}/deactivate")]
        [ApiKeyAuthorize(TargetTable = "Position", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeactivatePosition(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null) return NotFound();

            if (!position.Status) return BadRequest("El puesto ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? position.UpdatedByUserId;

            position.Status = false;
            position.UpdatedAt = DateTime.Now;
            position.UpdatedByUserId = userId;

            _context.Update(position);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Positions/5/activate  (reactivación LÓGICA)
        [HttpPut("{id}/activate")]
        [ApiKeyAuthorize(TargetTable = "Position", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> ActivatePosition(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position == null) return NotFound();

            if (position.Status) return BadRequest("El puesto ya está activo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? position.UpdatedByUserId;

            position.Status = true;
            position.UpdatedAt = DateTime.Now;
            position.UpdatedByUserId = userId;

            _context.Update(position);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
