using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;

namespace HumanResourcesProduction.Features.ApiKeys
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class ApiKeysApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;

        public ApiKeysApiController(HumanResourcesContext context)
        {
            _context = context;
        }

        public class ApiKeyCreateDto
        {
            public int UserId { get; set; }
            public List<PermissionAssignmentDto> Permissions { get; set; } = new();
        }

        public class PermissionAssignmentDto
        {
            public int TableId { get; set; }
            public int PermissionId { get; set; } // 1: Leer, 2: Escribir, 3: Actualizar, 4: Eliminar
        }

        // GET: api/ApiKeys
        [HttpGet]
        public async Task<IActionResult> GetApiKeys(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? userName = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.ApiKeys
                .Include(a => a.CreatedByUser)
                .Include(a => a.UpdatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userName))
                query = query.Where(a => a.CreatedByUser.Name.Contains(userName));

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.Key,
                    a.Status,
                    User = a.CreatedByUser != null ? a.CreatedByUser.Name : "Sistema",
                    a.CreatedAt,
                    a.UpdatedAt,
                    PermissionsCount = _context.ApiKeyTablePermissions.Count(p => p.ApiKeyId == a.Id && p.Status)
                })
                .ToListAsync();

            return Ok(new
            {
                items,
                totalItems,
                totalPages,
                page,
                pageSize
            });
        }

        // GET: api/ApiKeys/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApiKey(int id)
        {
            var apiKey = await _context.ApiKeys
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apiKey == null) return NotFound(new { message = "API Key no encontrada." });

            var permissions = await _context.ApiKeyTablePermissions
                .Where(p => p.ApiKeyId == id && p.Status)
                .Include(p => p.Table)
                .Include(p => p.ApiPermission)
                .Select(p => new
                {
                    p.TableId,
                    TableName = p.Table.Description,
                    p.ApiPermissionId,
                    PermissionName = p.ApiPermission.Description
                })
                .ToListAsync();

            return Ok(new
            {
                apiKey.Id,
                apiKey.Key,
                apiKey.Status,
                User = apiKey.CreatedByUser?.Name,
                apiKey.CreatedAt,
                permissions
            });
        }

        // POST: api/ApiKeys
        [HttpPost]
        public async Task<IActionResult> CreateApiKey([FromBody] ApiKeyCreateDto dto)
        {
            if (dto.UserId <= 0)
                return BadRequest(new { message = "El usuario propietario es requerido." });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest(new { message = "El usuario seleccionado no existe." });

            var key = Guid.NewGuid().ToString("N");

            var apiKey = new ApiKey
            {
                Key = key,
                Status = true,
                CreatedByUserId = dto.UserId,
                UpdatedByUserId = dto.UserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();

            if (dto.Permissions != null && dto.Permissions.Any())
            {
                foreach (var p in dto.Permissions)
                {
                    _context.ApiKeyTablePermissions.Add(new ApiKeyTablePermission
                    {
                        ApiKeyId = apiKey.Id,
                        TableId = p.TableId,
                        ApiPermissionId = p.PermissionId,
                        Status = true,
                        CreatedByUserId = dto.UserId,
                        UpdatedByUserId = dto.UserId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "API Key creada exitosamente.",
                apiKey.Id,
                apiKey.Key
            });
        }

        // PUT: api/ApiKeys/5/permissions  (reemplaza las reglas de acceso por tabla de la API Key)
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> UpdateApiKeyPermissions(int id, [FromBody] ApiKeyCreateDto dto)
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null) return NotFound(new { message = "API Key no encontrada." });

            var existing = await _context.ApiKeyTablePermissions
                .Where(p => p.ApiKeyId == id)
                .ToListAsync();

            if (existing.Any())
            {
                _context.ApiKeyTablePermissions.RemoveRange(existing);
                await _context.SaveChangesAsync();
            }

            if (dto.Permissions != null && dto.Permissions.Any())
            {
                var ownerId = apiKey.CreatedByUserId;
                var now = DateTime.Now;
                foreach (var p in dto.Permissions)
                {
                    _context.ApiKeyTablePermissions.Add(new ApiKeyTablePermission
                    {
                        ApiKeyId = id,
                        TableId = p.TableId,
                        ApiPermissionId = p.PermissionId,
                        Status = true,
                        CreatedByUserId = ownerId,
                        UpdatedByUserId = ownerId,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Permisos de la API Key actualizados exitosamente." });
        }

        // PUT: api/ApiKeys/5/deactivate
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateApiKey(int id)
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null) return NotFound(new { message = "API Key no encontrada." });

            apiKey.Status = false;
            apiKey.UpdatedAt = DateTime.Now;

            _context.Update(apiKey);
            await _context.SaveChangesAsync();

            return Ok(new { message = "API Key desactivada exitosamente." });
        }

        // PUT: api/ApiKeys/5  (actualizar propietario)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApiKey(int id, [FromBody] ApiKeyUpdateDto dto)
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null) return NotFound(new { message = "API Key no encontrada." });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return BadRequest(new { message = "El usuario seleccionado no existe." });

            apiKey.CreatedByUserId = dto.UserId;
            apiKey.UpdatedByUserId = dto.UserId;
            apiKey.UpdatedAt = DateTime.Now;

            _context.Update(apiKey);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Propietario actualizado correctamente." });
        }

        public class ApiKeyUpdateDto
        {
            public int UserId { get; set; }
        }
    }
}
