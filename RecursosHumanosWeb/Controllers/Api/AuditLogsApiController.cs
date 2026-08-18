using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;

namespace RecursosHumanosWeb.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;

        public AuditLogsApiController(HumanResourcesContext context)
        {
            _context = context;
        }

        // GET: api/AuditLogs
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int? tableId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.AuditLogs
                .Include(a => a.Table)
                .Include(a => a.ApiFunction)
                .Include(a => a.CreatedByUser)
                .Include(a => a.ApiKey)
                .Include(a => a.OriginType)
                .AsQueryable();

            if (dateFrom.HasValue)
                query = query.Where(a => a.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(a => a.CreatedAt <= dateTo.Value.Date.AddDays(1).AddSeconds(-1));

            if (tableId.HasValue)
                query = query.Where(a => a.TableId == tableId.Value);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    TableName = a.Table != null ? a.Table.Description : "N/A",
                    Action = a.ApiFunction != null ? a.ApiFunction.Description : "N/A",
                    UserName = a.CreatedByUser != null ? a.CreatedByUser.Name : "N/A",
                    Origin = a.OriginType != null ? a.OriginType.Description : "Desconocido",
                    ApiKeyName = a.ApiKey != null ? a.ApiKey.Key : null,
                    a.CreatedAt
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

        // GET: api/AuditLogs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLog(int id)
        {
            var audit = await _context.AuditLogs
                .Include(a => a.Table)
                .Include(a => a.ApiFunction)
                .Include(a => a.CreatedByUser)
                .Include(a => a.ApiKey)
                .Include(a => a.OriginType)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return NotFound(new { message = "Registro de auditoría no encontrado." });

            return Ok(new
            {
                audit.Id,
                TableName = audit.Table?.Description ?? "N/A",
                Action = audit.ApiFunction?.Description ?? "N/A",
                UserName = audit.CreatedByUser?.Name ?? "N/A",
                Origin = audit.OriginType?.Description ?? "Desconocido",
                audit.PreviousData,
                audit.CreatedAt
            });
        }
    }
}
