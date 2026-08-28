using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.ViewModels.Departments;
using X.PagedList.Extensions;

namespace HumanResourcesProduction.Controllers.Api.Departments
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
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? status = null,
            [FromQuery] int? areaId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Description.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (areaId.HasValue)
            {
                query = query.Where(d => d.AreaId == areaId.Value);
            }

            var paged = query
                .OrderBy(d => d.Description)
                .Select(d => new
                {
                    d.Id,
                    d.Description,
                    d.AreaId,
                    AreaDescription = d.AreaId != null
                        ? _context.Areas.Where(a => a.Id == d.AreaId).Select(a => a.Description).FirstOrDefault()
                        : null,
                    d.Status,
                    d.CreatedAt,
                    d.UpdatedAt
                })
                .ToPagedList(page, pageSize);

            return Ok(new
            {
                items = paged,
                totalItems = paged.TotalItemCount,
                totalPages = paged.PageCount,
                page = paged.PageNumber,
                pageSize = paged.PageSize
            });
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

            // Validación de relación: el área indicada debe existir y estar activa
            if (model.AreaId.HasValue &&
                !await _context.Areas.AnyAsync(a => a.Id == model.AreaId.Value && a.Status))
                return BadRequest(new { message = "El área especificada no existe o está inactiva." });

            // Regla de negocio: descripción única (no duplicados)
            if (await _context.Departments.AnyAsync(d => d.Description == description))
                return BadRequest(new { message = "Ya existe un departamento con esa descripción." });

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var department = _mapper.Map<Department>(model);
            department.Description = description;
            department.AreaId = model.AreaId;
            department.Status = true;
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

            // Validación de relación: el área indicada debe existir y estar activa
            if (model.AreaId.HasValue &&
                !await _context.Areas.AnyAsync(a => a.Id == model.AreaId.Value && a.Status))
                return BadRequest(new { message = "El área especificada no existe o está inactiva." });

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

        // PUT: api/Departments/5/activate  (reactivación lógica: inverso de deactivate)
        [HttpPut("{id}/activate")]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> ActivateDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            if (department.Status) return BadRequest("El department ya está activo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? department.UpdatedByUserId;

            department.Status = true;
            department.UpdatedAt = DateTime.Now;
            // Área se mantiene (no cambia al reactivar)
            department.UpdatedByUserId = userId;

            _context.Update(department);
            await _context.SaveChangesAsync();

            return NoContent();
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
