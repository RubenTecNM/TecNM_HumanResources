using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.ViewModels.Departments;

namespace RecursosHumanosWeb.Controllers.Api.Departments
{
    [Route("api/Departments")]
    [ApiController]
    public class DepartmentsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DepartmentsApiController> _logger;

        public DepartmentsApiController(HumanResourcesContext context, IMapper mapper, ILogger<DepartmentsApiController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Departments
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetDepartments(
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null)
        {
            var query = _context.Departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Description.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            var departments = await query
                .OrderBy(d => d.Description)
                .Select(d => new
                {
                    d.Id,
                    d.Description,
                    d.AreaId,
                    d.Status,
                    d.IsActive,
                    d.CreatedAt,
                    d.UpdatedAt
                })
                .ToListAsync();

            return Ok(departments);
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.Description,
                    d.AreaId,
                    d.Status,
                    d.IsActive,
                    d.CreatedAt,
                    d.UpdatedAt,
                    CreatedBy = d.CreatedByUser.Name,
                    UpdatedBy = d.UpdatedByUser.Name
                })
                .FirstOrDefaultAsync();

            if (department == null) return NotFound($"Department con ID {id} no encontrado.");

            return Ok(department);
        }

        // POST: api/Departments
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostDepartment([FromBody] DepartmentCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var description = model.Description?.Trim();

            // Regla de negocio: descripción única (no duplicados)
            if (await _context.Departments.AnyAsync(d => d.Description == description))
                return BadRequest(new { message = "Ya existe un departamento con esa descripción." });

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var department = _mapper.Map<Department>(model);
            department.Description = description;
            department.AreaId = model.AreaId;
            department.Status = true;
            department.IsActive = true;
            department.CreatedAt = DateTime.Now;
            department.UpdatedAt = DateTime.Now;
            department.CreatedByUserId = userId;
            department.UpdatedByUserId = userId;

            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, new { department.Id, department.Description });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear department vía API. Detalle: {Inner}", inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Departments/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutDepartment(int id, [FromBody] DepartmentUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var description = model.Description?.Trim();

            // Regla de negocio: descripción única (excluyendo al propio departamento)
            if (await _context.Departments.AnyAsync(d => d.Id != id && d.Description == description))
                return BadRequest(new { message = "Ya existe otro departamento con esa descripción." });

            var userId = HttpContext.Items["ApiUserId"] as int? ?? department.UpdatedByUserId;

            _mapper.Map(model, department);
            department.Description = description;
            department.AreaId = model.AreaId;
            department.UpdatedAt = DateTime.Now;
            department.UpdatedByUserId = userId;

            try
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar department {Id} vía API. Detalle: {Inner}", id, inner);
                return StatusCode(500, new { error = "Internal Server Error", message = ex.Message, detail = inner });
            }
        }

        // PUT: api/Departments/5/toggle-active  (alterna IsActive: Vigente / Reemplazado)
        [HttpPut("{id}/toggle-active")]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? department.UpdatedByUserId;
            var willBeActive = !department.IsActive;

            department.IsActive = willBeActive;
            department.UpdatedAt = DateTime.Now;
            // Área se mantiene (no cambia al toggle activo/inactivo)
            department.UpdatedByUserId = userId;

            _context.Update(department);
            await _context.SaveChangesAsync();

            var estado = willBeActive ? "Vigente" : "Reemplazado";
            return Ok(new { message = $"Departamento marcado como {estado}.", isActive = willBeActive });
        }

        // PUT: api/Departments/5/deactivate  (borrado LÓGICO: no existe DELETE físico)
        [HttpPut("{id}/deactivate")]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeactivateDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            if (!department.Status) return BadRequest("El department ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? department.UpdatedByUserId;

            department.Status = false;
            department.UpdatedAt = DateTime.Now;
            // Área se mantiene (no cambia al desactivar)
            department.UpdatedByUserId = userId;

            _context.Update(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
