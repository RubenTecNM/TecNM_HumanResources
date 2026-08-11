using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Filters;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Models.ViewModels.Permissions;

namespace RecursosHumanosWeb.Controllers.Api.Permissions
{
    [Route("api/Permissions")]
    [ApiController]
    public class PermissionsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;

        public PermissionsApiController(HumanResourcesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Permissions
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPermissions(int page = 1, int pageSize = 10)
        {
            var permissions = await _context.Permissions
                .Include(p => p.RequestingUser)
                .Include(p => p.PermissionType)
                .Where(p => p.PermissionStatusTypeId != (int)PermissionStatusEnum.InvalidatedByPeriod)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    Requester = p.RequestingUser.Name,
                    Type = p.PermissionType.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Reason,
                    p.PermissionStatusTypeId,
                    p.WithPay
                })
                .ToListAsync();

            return Ok(permissions);
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPermission(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.RequestingUser)
                .Include(p => p.PermissionType)
                .Include(p => p.Cutoff)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null) return NotFound($"Permission con ID {id} no encontrado.");

            // Mapeo manual rápido o DTO
            var result = new
            {
                permission.Id,
                Requester = permission.RequestingUser.Name,
                Type = permission.PermissionType.Description,
                permission.StartDate,
                permission.EndDate,
                permission.Reason,
                permission.Evidence, // URL relativa
                Status = permission.PermissionStatusTypeId,
                permission.WithPay,
                permission.CreatedAt,
                Cutoff = permission.Cutoff.StartDate.ToString("yyyy-MM-dd") + " - " + permission.Cutoff.EndDate.ToString("yyyy-MM-dd")
            };

            return Ok(result);
        }

        // POST: api/Permissions
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostPermission([FromBody] PermissionApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;
            
            // Asignar solicitante: si viene en el modelo y es válido, usarlo. Si no, usar el user de la API Key (si hace sentido, o error)
            // Aquí asumiremos que el user de la API Key puede solicitar para sí mismo, o si es Admin, para otros.
            // Para simplificar: si RequestingUserId es null, usamos userId.
            int requesterId = model.RequestingUserId ?? userId;

            // Encontrar Cutoff activo
            var activeCutoff = await _context.Cutoffs
                .Where(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (activeCutoff == null) return BadRequest("No hay un corte activo para registrar el permiso.");

            // Calcular días (lógica simple)
            int? days = null;
            if (model.StartDate != default && model.EndDate.HasValue)
            {
                days = (int)((model.EndDate.Value - model.StartDate).TotalDays + 1);
            }

            var permission = new Permission
            {
                Reason = model.Reason,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                PermissionTypeId = model.PermissionTypeId,
                RequestingUserId = requesterId,
                CutoffId = activeCutoff.Id,
                WithPay = model.WithPay,
                PermissionStatusTypeId = (int)PermissionStatusEnum.Pending,
                Days = days,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedByUserId = userId,
                UpdatedByUserId = userId
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, new { permission.Id });
        }

        // PUT: api/Permissions/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutPermission(int id, [FromBody] PermissionApiUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            if (permission.PermissionStatusTypeId != (int)PermissionStatusEnum.Pending) return BadRequest("No se puede modificar un permiso ya revisado o invalidado.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? permission.UpdatedByUserId;

            // Actualizar campos
            permission.Reason = model.Reason;
            permission.StartDate = model.StartDate;
            permission.EndDate = model.EndDate;
            permission.PermissionTypeId = model.PermissionTypeId;
            permission.WithPay = model.WithPay;
            
            permission.UpdatedAt = DateTime.Now;
            permission.UpdatedByUserId = userId;

             // Recalcular días
            if (permission.StartDate.HasValue && permission.EndDate.HasValue)
            {
                permission.Days = (int)((permission.EndDate.Value - permission.StartDate.Value).TotalDays + 1);
            }

            _context.Update(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            if (permission.PermissionStatusTypeId == (int)PermissionStatusEnum.InvalidatedByPeriod) return BadRequest("El permiso ya está invalidado.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? permission.UpdatedByUserId;

            permission.PermissionStatusTypeId = (int)PermissionStatusEnum.InvalidatedByPeriod; // Invalidated
            permission.UpdatedAt = DateTime.Now;
            permission.UpdatedByUserId = userId;

            _context.Update(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
