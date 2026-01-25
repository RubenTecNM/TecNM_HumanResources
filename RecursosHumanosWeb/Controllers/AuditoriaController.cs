using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using X.PagedList.Extensions;

namespace RecursosHumanosWeb.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditoriaController : Controller
    {
        private readonly RecursosHumanosContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AuditoriaController> _logger;

        public AuditoriaController(RecursosHumanosContext context, IMapper mapper, ILogger<AuditoriaController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? idTabla = null)
        {
            try
            {
                var query = _context.Auditorias
                    .Include(a => a.IdTablaNavigation)
                    .Include(a => a.IdApiFunctionNavigation)
                    .Include(a => a.IdUsuarioCreaNavigation)
                    .Include(a => a.IdApiKeyNavigation)
                    .OrderByDescending(a => a.FechaCreacion)
                    .AsQueryable();

                // Filtros
                if (fechaDesde.HasValue)
                    query = query.Where(a => a.FechaCreacion >= fechaDesde.Value);
                if (fechaHasta.HasValue)
                    query = query.Where(a => a.FechaCreacion <= fechaHasta.Value.AddDays(1).AddSeconds(-1));
                if (idTabla.HasValue)
                    query = query.Where(a => a.IdTabla == idTabla.Value);

                var pagedList = query
                    .ProjectTo<AuditoriaDTO>(_mapper.ConfigurationProvider)
                    .ToPagedList(page, pageSize);

                ViewBag.IdTablaList = await _context.Tablas
                    .Where(t => t.Estatus)
                    .OrderBy(t => t.Descripcion)
                    .ToListAsync();
                
                ViewBag.FechaDesde = fechaDesde;
                ViewBag.FechaHasta = fechaHasta;
                ViewBag.IdTabla = idTabla;

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
            var audit = await _context.Auditorias
                .Include(a => a.IdTablaNavigation)
                .Include(a => a.IdApiFunctionNavigation)
                .Include(a => a.IdUsuarioCreaNavigation)
                .Include(a => a.IdApiKeyNavigation)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null) return NotFound();

            return PartialView("_Details", audit);
        }
    }
}
