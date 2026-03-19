using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList.Extensions;
using AutoMapper.QueryableExtensions;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Services;
using System.Security.Claims;

namespace RecursosHumanosWeb.Controllers
{
    [Authorize(Policy = "RHOrAdmin")]
    public class CorteController : Controller
    {
        private readonly RecursosHumanosContext _context;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ILogger<CorteController> _logger;
        private readonly ICorteService _corteService;
        private readonly IExportService _exportService;

        public CorteController(
            RecursosHumanosContext context, 
            AutoMapper.IMapper mapper, 
            ILogger<CorteController> logger,
            ICorteService corteService,
            IExportService exportService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _corteService = corteService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, int? idFilter = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null, int? usuarioCreadorFilter = null, int? correoCreadorFilter = null, bool? statusFilter = null)
        {
            // Validar parámetros de paginación
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 5;

            // Obtener información del usuario autenticado
            bool isAdministrador = User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.Administrador).ToString());
            bool isRecursosHumanos = User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.RecursosHumanos).ToString());

            // Construir la consulta base
            var cortesQuery = _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .Include(c => c.IdUsuarioModificaNavigation)
                .AsQueryable();

            if (idFilter.HasValue)
            {
                cortesQuery = cortesQuery.Where(c => c.Id == idFilter.Value);
            }

            // Cierre automático lazy
            await _corteService.AutoCloseCorteIfNeededAsync();

            // Aplicar filtros
            if (fechaDesde.HasValue || fechaHasta.HasValue)
            {
                // Un corte toca el rango si su periodo (Inicia a Termina) se solapa con el rango buscado.
                var d = fechaDesde ?? new DateTime(1900, 1, 1);
                var a = fechaHasta?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(2100, 12, 31);
                
                cortesQuery = cortesQuery.Where(c => c.Inicia <= a && c.Termina >= d);
            }
            if (usuarioCreadorFilter.HasValue)
            {
                cortesQuery = cortesQuery.Where(c => c.IdUsuarioCrea == usuarioCreadorFilter.Value);
            }
            if (correoCreadorFilter.HasValue)
            {
                cortesQuery = cortesQuery.Where(c => c.IdUsuarioCrea == correoCreadorFilter.Value);
            }
            if (statusFilter.HasValue)
            {
                cortesQuery = cortesQuery.Where(c => c.Estatus == statusFilter.Value);
            }

            // Nota: ya no obtenemos la lista completa de usuarios aquí.
            // El filtro de usuario en la vista utilizará un endpoint centralizado en UsuariosController (SearchUsers)
            var usuarios = new List<UsuariosDTO>();

            // Preparar la consulta filtrada usando AutoMapper ProjectTo a CortesDTO
            var cortesQueryFiltered = cortesQuery
                .ProjectTo<CortesDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(c => c.Id); // Ordenar por Id descendente

            // Obtener resultados paginados
            var cortes = cortesQueryFiltered.ToPagedList(page, pageSize);

            // Preparar el ViewModel
            var model = new SearchCorteViewModel
            {
                Cortes = cortes,
                Usuarios = usuarios,
                FechaDesde = fechaDesde?.ToString("yyyy-MM-dd")!,
                FechaHasta = fechaHasta?.ToString("yyyy-MM-dd")!,
                UsuarioCreadorFilter = usuarioCreadorFilter?.ToString()!,
                CorreoCreadorFilter = correoCreadorFilter?.ToString()!,
                StatusFilter = statusFilter?.ToString()!,
                IsAdministrador = isAdministrador
            };

            // Pasar datos a la vista
            ViewData["IsRH"] = isRecursosHumanos; // Añadido para que la vista lo use
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.UsuarioCreadorFilter = usuarioCreadorFilter;
            ViewBag.CorreoCreadorFilter = correoCreadorFilter;
            ViewBag.StatusFilter = statusFilter;

            if (usuarioCreadorFilter.HasValue)
            {
                var u = await _context.Usuarios.FindAsync(usuarioCreadorFilter.Value);
                ViewBag.SelectedNombreText = u?.Nombre;
            }
            if (correoCreadorFilter.HasValue)
            {
                var u = await _context.Usuarios.FindAsync(correoCreadorFilter.Value);
                ViewBag.SelectedCorreoText = u?.Correo;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Close(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            bool result = await _corteService.CloseCorteAsync(id, userId);
            if (result)
            {
                TempData["SuccessAlert"] = "Periodo cerrado exitosamente. Los permisos pendientes han sido rechazados automáticamente.";
            }
            else
            {
                TempData["ErrorAlert"] = "No se pudo cerrar el periodo. Verifique si ya está inactivo.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int id)
        {
            var content = await _exportService.GenerateCorteExcelAsync(id);
            if (content == null) return NotFound("Corte no encontrado");

            var corte = await _context.Cortes.FindAsync(id);
            string fileName = $"CorteTEC - ID {id}.xlsx";

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToCsv(int id)
        {
            var result = await _exportService.GenerateCorteCsvAsync(id);
            if (result.Content == null) return NotFound("Corte no encontrado");

            return File(result.Content, "text/csv", result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToJson(int id)
        {
            var result = await _exportService.GenerateCorteJsonAsync(id);
            if (result.Content == null) return NotFound("Corte no encontrado");

            return File(result.Content, "application/json", result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToXml(int id)
        {
            var result = await _exportService.GenerateCorteXmlAsync(id);
            if (result.Content == null) return NotFound("Corte no encontrado");

            return File(result.Content, "application/xml", result.FileName);
        }
        // GET: Cortes/Create
        public async Task<IActionResult> Create()
        {
            // Preparar información de usuario para la vista y site.js
            ViewData["IsAuthenticated"] = User.Identity?.IsAuthenticated ?? false;
            ViewData["UserName"] = User.Identity?.Name ?? string.Empty;
            ViewBag.IsRH = User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.RecursosHumanos).ToString());
            ViewBag.IsAdministrador = User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.Administrador).ToString());
            ViewData["IdUsuario"] = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Cierre automático lazy
            await _corteService.AutoCloseCorteIfNeededAsync();

            var viewModel = new CorteCreateViewModel();

            // Buscar el último corte (activo o no) para basar la fecha de inicio
            var ultimoCorte = await _corteService.GetLastCorteAsync();
            
            if (ultimoCorte != null)
            {
                // El nuevo corte inicia el día siguiente al fin del anterior
                viewModel.Inicia = ultimoCorte.Termina.Date.AddDays(1);
            }
            else
            {
                // Si es el primer corte del sistema, usamos hoy
                viewModel.Inicia = DateTime.Now.Date;
            }

            // Fecha de término por defecto es el día actual, 
            // pero asegurando que sea al menos igual o posterior al inicio para evitar conflictos inmediatos
            viewModel.Termina = DateTime.Now.Date > viewModel.Inicia ? DateTime.Now.Date : viewModel.Inicia.AddDays(2);
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CorteCreateViewModel viewModel)
        {
            // Verificar que el usuario esté autenticado
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int idUsuario = int.Parse(userIdStr);

            // Validaciones estándar
            if (viewModel.Termina <= viewModel.Inicia)
                ModelState.AddModelError("Termina", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            
            var corteActivo = await _corteService.GetActiveCorteAsync();
            if (corteActivo != null && viewModel.Termina <= corteActivo.Termina)
                ModelState.AddModelError("Termina", "La fecha de fin debe ser posterior al fin del corte actual.");

            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var nuevoCorte = new Corte
                {
                    Inicia = viewModel.Inicia,
                    Termina = viewModel.Termina
                };

                var success = await _corteService.CreateCorteAsync(nuevoCorte, idUsuario);
                if (!success)
                {
                    ModelState.AddModelError("", "No se puede crear un nuevo corte porque el corte actual tiene permisos pendientes de revisión.");
                    return View(viewModel);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear corte.");
                ModelState.AddModelError("", $"Error al guardar el corte: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Cortes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var corte = await _context.Cortes.FindAsync(id);
            if (corte == null)
            {
                return NotFound();
            }
            ViewData["IdUsuarioCrea"] = new SelectList(_context.Usuarios, "Id", "Nombre", corte.IdUsuarioCrea);
            ViewData["IdUsuarioModifica"] = new SelectList(_context.Usuarios, "Id", "Nombre", corte.IdUsuarioModifica);
            return View(corte);
        }

        // POST: Cortes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Inicia,Termina,FechaCreacion,FechaModificacion,Estatus,IdUsuarioCrea,IdUsuarioModifica")] Corte corte)
        {
            if (id != corte.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    corte.FechaModificacion = DateTime.Now; // FechaModificacion remains DateTime
                    _context.Update(corte);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cortes.Any(e => e.Id == corte.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    var inner = ex.InnerException?.Message ?? "No inner exception";
                    _logger.LogError(ex, "Error al editar corte {Id}. Detalle: {Inner}", corte.Id, inner);
                    ModelState.AddModelError("", $"Error al actualizar el corte: {ex.Message}. Detalle: {inner}");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUsuarioCrea"] = new SelectList(_context.Usuarios, "Id", "Nombre", corte.IdUsuarioCrea);
            ViewData["IdUsuarioModifica"] = new SelectList(_context.Usuarios, "Id", "Nombre", corte.IdUsuarioModifica);
            return View(corte);
        }

        // GET: Cortes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var corte = await _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .Include(c => c.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (corte == null)
            {
                return NotFound();
            }

            return View(corte);
        }

        // POST: Corte/Delete/5 - borrado lógico
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int idUsuario = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var success = await _corteService.InactivateCorteAsync(id, idUsuario);
            if (!success)
            {
                TempData["ErrorAlert"] = JsonConvert.SerializeObject(new { icon = "error", title = "Error", text = "No se puede cerrar el corte o no existe." });
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Delete via fetch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax([FromBody] ActionRequestDTO request)
        {
            if (request == null || request.Id <= 0)
                return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "ID no válido.", Icon = "error" });

            if (!User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.Administrador).ToString()))
                return Json(new AlertResponseDTO { Success = false, Title = "Acceso denegado", Message = "No tiene permisos para eliminar cortes.", Icon = "error" });

            var corte = await _context.Cortes.FindAsync(request.Id);
            if (corte == null)
                return Json(new AlertResponseDTO { Success = false, Title = "No encontrado", Message = "Corte no encontrado.", Icon = "error" });

            if (!request.Confirmed)
            {
                return Json(new AlertResponseDTO
                {
                    ShowConfirmation = true,
                    Title = "¿Eliminar Corte?",
                    Message = $"¿Estás seguro de que deseas eliminar el corte que inicia {corte.Inicia.ToString("dd/MM/yyyy")}?",
                    Icon = "question",
                    ConfirmButtonText = "Sí, eliminar"
                });
            }

            int idUsuario = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _corteService.InactivateCorteAsync(request.Id, idUsuario);

            if (success)
            {
                return Json(new AlertResponseDTO { Success = true, Title = "Eliminado", Message = "El corte fue inactivado correctamente.", Icon = "success", RedirectUrl = "/Corte/Index" });
            }
            else
            {
                return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "Ocurrió un error al inactivar.", Icon = "error" });
            }
        }


        // Reemplaza tu método [HttpGet] Details(int? id) existente por este:

       [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 1. Obtener el corte y sus datos de auditoría
            var corte = await _context.Cortes
                .Include(c => c.IdUsuarioCreaNavigation)
                .Include(c => c.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (corte == null) return NotFound();

            // 2. Obtener los permisos activos para las estadísticas
            var permisos = await _context.Permisos
                .Where(p => p.IdCorte == id && p.Estatus != false)
                .Include(p => p.IdTipoPermisoNavigation)
                .Include(p => p.IdUsuarioSolicitaNavigation)
                    .ThenInclude(u => u.IdAreaNavigation)
                .ToListAsync();

            // 3. Paleta de colores para las gráficas
            string[] palette = new string[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899" };

            // 4. Procesar Datos para JSON (Gráficas)
            var permisosPorArea = permisos
                .Where(p => p.IdUsuarioSolicitaNavigation?.IdAreaNavigation != null)
                .GroupBy(p => p.IdUsuarioSolicitaNavigation!.IdAreaNavigation!)
                .Select((g, index) => new {
                    Id = g.Key.Id,
                    Name = g.Key.Descripcion,
                    Value = g.Count(),
                    Color = palette[index % palette.Length]
                }).OrderByDescending(x => x.Value).ToList();

            var permisosPorTipo = permisos
                .GroupBy(p => p.IdTipoPermisoNavigation!)
                .Select((g, index) => new {
                    Name = g.Key?.Descripcion ?? "Otros",
                    Value = g.Count(),
                    Color = palette[(index + 2) % palette.Length]
                }).ToList();

            var tendencia = permisos
                .OrderBy(p => p.FechaCreacion)
                .GroupBy(p => p.FechaCreacion.ToString("dd MMM"))
                .Select(g => new {
                    Month = g.Key ?? "S/F",
                    Total = g.Count(),
                    Aprobados = g.Count(x => x.Goce == true),
                    Pendientes = g.Count(x => x.Goce != true)
                }).ToList();

            var heatmap = permisos
                .GroupBy(p => new { 
                    Day = p.FechaCreacion.ToString("dddd", new CultureInfo("en-US")), 
                    Week = (p.FechaCreacion.Day / 7) + 1 
                })
                .Select(g => new { Day = g.Key.Day, Week = g.Key.Week, Count = g.Count() })
                .ToList();

            // 5. Mapear al DTO
            var model = new CorteDetailsDTO
            {
                Id = corte.Id,
                Inicia = corte.Inicia,
                Termina = corte.Termina,
                Estatus = corte.Estatus,
                UsuarioCrea = corte.IdUsuarioCreaNavigation?.Nombre ?? "Sistema",
                UsuarioModifica = corte.IdUsuarioModificaNavigation?.Nombre,
                TotalPermisos = permisos.Count,
                ConGoce = permisos.Count(p => p.Goce == true),
                SinGoce = permisos.Count(p => p.Goce != true),
                DiasPromedio = permisos.Any() ? permisos.Average(p => p.Dias ?? 0) : 0,
                
                // Serialización
                PermisosPorTipoJson = JsonConvert.SerializeObject(permisosPorTipo),
                PermisosPorAreaJson = JsonConvert.SerializeObject(permisosPorArea),
                TendenciaJson = JsonConvert.SerializeObject(tendencia),
                HeatmapDataJson = JsonConvert.SerializeObject(heatmap),
                KpiDataJson = JsonConvert.SerializeObject(new { total = permisos.Count })
            };

            return View(model);
        }
        // Método auxiliar para obtener el número de semana
        private int GetWeekNumber(DateTime date)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date,
                CalendarWeekRule.FirstDay,
                DayOfWeek.Monday
            );
        }
    }
}