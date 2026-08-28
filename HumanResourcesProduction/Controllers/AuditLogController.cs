using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using X.PagedList.Extensions;

namespace HumanResourcesProduction.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditLogController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuditLogController> _logger;

        public AuditLogController(HumanResourcesContext context, IMapper mapper, ILogger<AuditLogController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, DateTime? dateFrom = null, DateTime? dateTo = null, int? tableId = null)
        {
            try
            {
                var query = _context.AuditLogs
                    .Include(a => a.Table)
                    .Include(a => a.ApiFunction)
                    .Include(a => a.CreatedByUser)
                    .Include(a => a.ApiKey)
                    .Include(a => a.OriginType)
                    .OrderByDescending(a => a.CreatedAt)
                    .AsQueryable();

                // Filtros
                if (dateFrom.HasValue)
                    query = query.Where(a => a.CreatedAt >= dateFrom.Value);
                if (dateTo.HasValue)
                    query = query.Where(a => a.CreatedAt <= dateTo.Value.AddDays(1).AddSeconds(-1));
                if (tableId.HasValue)
                    query = query.Where(a => a.TableId == tableId.Value);

                var pagedList = query
                    .ProjectTo<AuditLogDTO>(_mapper.ConfigurationProvider)
                    .ToPagedList(page, pageSize);

                ViewBag.TableIdList = await _context.DatabaseTables
                    .Where(t => t.Status)
                    .OrderBy(t => t.Description)
                    .ToListAsync();
                
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                ViewBag.TableId = tableId;

                return View(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la tabla de auditoría.");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var audit = await _context.AuditLogs
                .Include(a => a.Table)
                .Include(a => a.ApiFunction)
                .Include(a => a.CreatedByUser)
                .Include(a => a.ApiKey)
                .Include(a => a.OriginType)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return NotFound();

            return PartialView("_Details", audit);
        }
    }
}
