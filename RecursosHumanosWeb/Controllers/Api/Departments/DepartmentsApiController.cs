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
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Where(d => d.Status == true)
                .Select(d => new
                {
                    d.Id,
                    d.Description
                })
                .ToListAsync();

            if (!departments.Any()) return NotFound("No se encontraron departments activos.");

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

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var department = _mapper.Map<Department>(model);
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

            var userId = HttpContext.Items["ApiUserId"] as int? ?? department.UpdatedByUserId;

            _mapper.Map(model, department);
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

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TargetTable = "Department", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();

            if (!department.Status) return BadRequest("El department ya está inactivo.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? department.UpdatedByUserId;

            department.Status = false;
            department.UpdatedAt = DateTime.Now;
            department.UpdatedByUserId = userId;

            _context.Update(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
