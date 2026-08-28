using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.Enums;
using HumanResourcesProduction.Models.ViewModels.Cutoffs;
using HumanResourcesProduction.Services;
using System.Globalization;

namespace HumanResourcesProduction.Controllers.Api.Cutoffs
{
    [Route("api/Cutoffs")]
    [ApiController]
    public class CutoffsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ICutoffService _cutoffService;
        private readonly IExportService _exportService;

        public CutoffsApiController(HumanResourcesContext context, IMapper mapper, ICutoffService cutoffService, IExportService exportService)
        {
            _context = context;
            _mapper = mapper;
            _cutoffService = cutoffService;
            _exportService = exportService;
        }

        // GET: api/Cutoffs
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetCutoffs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] int? statusId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .Include(c => c.CutoffStatusType)
                .AsQueryable();

            if (dateFrom.HasValue || dateTo.HasValue)
            {
                var d = dateFrom ?? new DateTime(1900, 1, 1);
                var a = dateTo?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(2100, 12, 31);
                query = query.Where(c => c.StartDate <= a && c.EndDate >= d);
            }

            if (statusId.HasValue)
            {
                query = query.Where(c => c.CutoffStatusTypeId == statusId.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.StartDate,
                    c.EndDate,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.CutoffStatusTypeId,
                    StatusDescription = c.CutoffStatusType != null ? c.CutoffStatusType.Description : "N/A",
                    StatusColor = c.CutoffStatusType != null ? c.CutoffStatusType.Color : "#6c757d",
                    CreatedBy = c.CreatedByUser != null ? c.CreatedByUser.Name : "Sistema",
                    UpdatedBy = c.UpdatedByUser != null ? c.UpdatedByUser.Name : null,
                    PermissionsCount = _context.Permissions.Count(p => p.CutoffId == c.Id)
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

        // GET: api/Cutoffs/suggested-dates
        [HttpGet("suggested-dates")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetSuggestedDates()
        {
            var lastCutoff = await _context.Cutoffs
                .OrderByDescending(c => c.EndDate)
                .FirstOrDefaultAsync();

            DateTime startDate = lastCutoff != null ? lastCutoff.EndDate.Date.AddDays(1) : DateTime.Now.Date;
            DateTime endDate = DateTime.Now.Date > startDate ? DateTime.Now.Date : startDate.AddDays(14);

            return Ok(new
            {
                startDate,
                endDate
            });
        }

        // GET: api/Cutoffs/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetCutoff(int id)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CutoffStatusType)
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (cutoff == null) return NotFound(new { message = $"El periodo con ID {id} no existe." });

            return Ok(new
            {
                cutoff.Id,
                cutoff.StartDate,
                cutoff.EndDate,
                cutoff.CutoffStatusTypeId,
                StatusDescription = cutoff.CutoffStatusType?.Description,
                StatusColor = cutoff.CutoffStatusType?.Color,
                cutoff.CreatedAt,
                cutoff.UpdatedAt,
                CreatedBy = cutoff.CreatedByUser?.Name ?? "Sistema",
                UpdatedBy = cutoff.UpdatedByUser?.Name
            });
        }

        // GET: api/Cutoffs/5/analytics
        [HttpGet("{id}/analytics")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetCutoffAnalytics(int id)
        {
            var cutoff = await _context.Cutoffs
                .Include(c => c.CutoffStatusType)
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cutoff == null) return NotFound(new { message = "Periodo no encontrado." });

            var permissions = await _context.Permissions
                .Where(p => p.CutoffId == id)
                .Include(p => p.PermissionType)
                .Include(p => p.RequestingUser)
                    .ThenInclude(u => u.Area)
                .ToListAsync();

            string[] palette = new string[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899" };

            var permissionsByArea = permissions
                .Where(p => p.RequestingUser?.Area != null)
                .GroupBy(p => p.RequestingUser!.Area!)
                .Select((g, index) => new
                {
                    id = g.Key.Id,
                    name = g.Key.Description,
                    value = g.Count(),
                    color = palette[index % palette.Length]
                })
                .OrderByDescending(x => x.value)
                .ToList();

            var permissionsByType = permissions
                .GroupBy(p => p.PermissionType!)
                .Select((g, index) => new
                {
                    name = g.Key?.Description ?? "Otros",
                    value = g.Count(),
                    color = palette[(index + 2) % palette.Length]
                })
                .ToList();

            // Serie temporal por día calendario (ISO): el frontend expande el rango completo del corte.
            var trend = permissions
                .OrderBy(p => p.CreatedAt)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    total = g.Count(),
                    approved = g.Count(x => x.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved),
                    pending = g.Count(x => x.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending),
                    rejected = g.Count(x => x.PermissionStatusTypeId == (int)PermissionStatusEnum.Rejected)
                })
                .ToList();

            var topRequesters = permissions
                .Where(p => p.RequestingUser != null)
                .GroupBy(p => p.RequestingUser!)
                .Select(g => new
                {
                    name = g.Key.Name,
                    area = g.Key.Area?.Description,
                    value = g.Count()
                })
                .OrderByDescending(x => x.value)
                .Take(5)
                .ToList();

            return Ok(new
            {
                cutoff = new
                {
                    cutoff.Id,
                    cutoff.StartDate,
                    cutoff.EndDate,
                    cutoff.CutoffStatusTypeId,
                    StatusDescription = cutoff.CutoffStatusType?.Description,
                    StatusColor = cutoff.CutoffStatusType?.Color,
                    CreatedBy = cutoff.CreatedByUser?.Name ?? "Sistema",
                    UpdatedBy = cutoff.UpdatedByUser?.Name
                },
                kpis = new
                {
                    totalPermissions = permissions.Count,
                    withPay = permissions.Count(p => p.WithPay == true),
                    withoutPay = permissions.Count(p => p.WithPay != true),
                    averageDays = permissions.Any()
                        ? Math.Round(permissions.Average(p =>
                        {
                            var inicio = (p.StartDate ?? DateTime.Today).Date;
                            var fin = (p.EndDate ?? inicio).Date;
                            return p.Days ?? (fin - inicio).Days + 1;
                        }), 1)
                        : 0,
                    pendingCount = permissions.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Pending),
                    approvedCount = permissions.Count(p => p.PermissionStatusTypeId == (int)PermissionStatusEnum.Approved)
                },
                permissionsByArea,
                permissionsByType,
                trend,
                topRequesters
            });
        }

        // GET: api/Cutoffs/5/export?format=excel|csv|json|xml   (reemplaza los 4 exports del MVC legacy)
        [HttpGet("{id}/export")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Leer")]
        public async Task<IActionResult> ExportCutoff(int id, [FromQuery] string format = "excel")
        {
            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return NotFound(new { message = "Periodo no encontrado." });

            switch (format?.ToLowerInvariant())
            {
                case "csv":
                    var csv = await _exportService.GenerateCutoffCsvAsync(id);
                    if (csv.Content == null) return NotFound("Periodo no encontrado");
                    return File(csv.Content, "text/csv", csv.FileName);

                case "json":
                    var json = await _exportService.GenerateCutoffJsonAsync(id);
                    if (json.Content == null) return NotFound("Periodo no encontrado");
                    return File(json.Content, "application/json", json.FileName);

                case "xml":
                    var xml = await _exportService.GenerateCutoffXmlAsync(id);
                    if (xml.Content == null) return NotFound("Periodo no encontrado");
                    return File(xml.Content, "application/xml", xml.FileName);

                case "excel":
                default:
                    var excel = await _exportService.GenerateCutoffExcelAsync(id);
                    if (excel == null) return NotFound("Periodo no encontrado");
                    return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CorteTEC - ID {id}.xlsx");
            }
        }

        // POST: api/Cutoffs
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostCutoff([FromBody] CutoffApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (model.EndDate <= model.StartDate)
            {
                return BadRequest(new { message = "La fecha de fin no puede ser anterior o igual a la de inicio." });
            }

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 1;

            var cutoff = _mapper.Map<Cutoff>(model);

            // El servicio valida las reglas de negocio en transacción:
            // cierre del periodo anterior (si no tiene pendientes), prohibición de traslapes,
            // normalización de fechas y creación con estado Open.
            bool created = await _cutoffService.CreateCutoffAsync(cutoff, userId);
            if (!created)
            {
                bool traslapa = await _context.Cutoffs.AnyAsync(c =>
                    (cutoff.StartDate >= c.StartDate && cutoff.StartDate <= c.EndDate) ||
                    (cutoff.EndDate >= c.StartDate && cutoff.EndDate <= c.EndDate) ||
                    (c.StartDate >= cutoff.StartDate && c.StartDate <= cutoff.EndDate));

                if (traslapa)
                    return BadRequest(new { message = "El periodo se encima con un periodo existente." });

                return BadRequest(new { message = "No se puede crear un nuevo periodo mientras el periodo activo actual tenga permisos pendientes por revisar." });
            }

            return CreatedAtAction(nameof(GetCutoff), new { id = cutoff.Id }, new { cutoff.Id, cutoff.StartDate, cutoff.EndDate });
        }

        // PUT: api/Cutoffs/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutCutoff(int id, [FromBody] CutoffApiUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return NotFound(new { message = "El periodo especificado no fue encontrado." });

            // Regla de negocio: solo se editan periodos abiertos
            if (cutoff.CutoffStatusTypeId != (int)CutoffStatusEnum.Open)
                return BadRequest(new { message = "Solo se pueden editar periodos en estado abierto." });

            if (model.EndDate <= model.StartDate)
                return BadRequest(new { message = "La fecha de fin no puede ser anterior o igual a la de inicio." });

            // Regla de negocio: el periodo no puede solaparse con otro existente (excluyéndose a sí mismo)
            bool traslapa = await _context.Cutoffs.AnyAsync(c => c.Id != id &&
                (model.StartDate >= c.StartDate && model.StartDate <= c.EndDate) ||
                (model.EndDate >= c.StartDate && model.EndDate <= c.EndDate) ||
                (c.StartDate >= model.StartDate && c.StartDate <= model.EndDate));

            if (traslapa)
                return BadRequest(new { message = "El periodo se encima con otro periodo existente." });

            var userId = HttpContext.Items["ApiUserId"] as int? ?? cutoff.UpdatedByUserId;

            _mapper.Map(model, cutoff);
            cutoff.StartDate = cutoff.StartDate.Date;
            cutoff.EndDate = cutoff.EndDate.Date.AddDays(1).AddTicks(-1);
            cutoff.UpdatedAt = DateTime.Now;
            cutoff.UpdatedByUserId = userId;

            _context.Update(cutoff);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Periodo actualizado exitosamente." });
        }

        // PUT: api/Cutoffs/5/close
        [HttpPut("{id}/close")]
        [ApiKeyAuthorize(TargetTable = "Cutoff", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> CloseCutoff(int id)
        {
            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null) return NotFound(new { message = "Periodo no encontrado." });

            if (cutoff.CutoffStatusTypeId == (int)CutoffStatusEnum.Closed)
                return BadRequest(new { message = "El corte ya se encuentra cerrado." });

            var userId = HttpContext.Items["ApiUserId"] as int? ?? cutoff.UpdatedByUserId;

            // El servicio cierra y rechaza automáticamente los permisos pendientes en transacción
            bool closed = await _cutoffService.CloseCutoffAsync(id, userId ?? 0);
            if (!closed)
                return BadRequest(new { message = "No se pudo cerrar el periodo. Verifique que esté en estado abierto." });

            return Ok(new { message = "El periodo ha sido cerrado exitosamente." });
        }
    }
}
