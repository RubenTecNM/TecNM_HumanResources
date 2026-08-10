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
    public class CutoffController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ILogger<CutoffController> _logger;
        private readonly ICutoffService _cutoffService;
        private readonly IExportService _exportService;
        private readonly IConfiguration _configuration;

        public CutoffController(
            HumanResourcesContext context, 
            AutoMapper.IMapper mapper, 
            ILogger<CutoffController> logger,
            ICutoffService cutoffService,
            IExportService exportService,
            IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cutoffService = cutoffService;
            _exportService = exportService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int? pageSize = null, DateTime? dateFrom = null, DateTime? dateTo = null, int? statusId = null)
        {
            int defaultPageSize = _configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize");
            int maxPageSize = _configuration.GetValue<int>("BusinessRules:Pagination:MaxPageSize");
            
            int actualPageSize = pageSize ?? defaultPageSize;
            
            // Validar parámetros de paginación
            if (page < 1) page = 1;
            if (actualPageSize < 1 || actualPageSize > maxPageSize) actualPageSize = defaultPageSize;

            // Obtener información del user autenticado
            bool isAdministrator = User.HasClaim("UserType", ((int)UserTypeEnum.Administrator).ToString());
            bool isHumanResources = User.HasClaim("UserType", ((int)UserTypeEnum.HumanResources).ToString());

            // Construir la consulta base
            var cutoffsQuery = _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .AsQueryable();

            // Cierre automático lazy
            await _cutoffService.AutoCloseCutoffIfNeededAsync();

            // Aplicar filtros de fecha (Vigencia)
            if (dateFrom.HasValue || dateTo.HasValue)
            {
                // Un cutoff toca el rango si su periodo (StartDate a EndDate) se solapa con el rango buscado.
                var d = dateFrom ?? new DateTime(1900, 1, 1);
                var a = dateTo?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(2100, 12, 31);
                
                cutoffsQuery = cutoffsQuery.Where(c => c.StartDate <= a && c.EndDate >= d);
            }

            if (statusId.HasValue)
            {
                cutoffsQuery = cutoffsQuery.Where(c => c.CutoffStatusTypeId == statusId.Value);
            }

            // Preparar la consulta filtrada usando AutoMapper ProjectTo a CutoffsDTO
            var cutoffsQueryFiltered = cutoffsQuery
                .ProjectTo<CutoffsDTO>(_mapper.ConfigurationProvider)
                .OrderByDescending(c => c.Id);

            // Obtener resultados paginados
            var cutoffs = cutoffsQueryFiltered.ToPagedList(page, actualPageSize);

            // Preparar el ViewModel
            var model = new SearchCutoffViewModel
            {
                Cutoffs = cutoffs,
                Users = new List<UsersDTO>(), // No longer needed for filters but keeping temporarily for DTO consistency if needed
                DateFrom = dateFrom?.ToString("yyyy-MM-dd")!,
                DateTo = dateTo?.ToString("yyyy-MM-dd")!,
                StatusId = statusId,
                IsAdministrator = isAdministrator
            };

            // Pasar datos a la vista
            ViewData["IsRH"] = isHumanResources;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.StatusId = statusId;

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int id)
        {
            var content = await _exportService.GenerateCutoffExcelAsync(id);
            if (content == null) return NotFound("Cutoff no encontrado");

            var cutoff = await _context.Cutoffs.FindAsync(id);
            string fileName = $"CorteTEC - ID {id}.xlsx";

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToCsv(int id)
        {
            var result = await _exportService.GenerateCutoffCsvAsync(id);
            if (result.Content == null) return NotFound("Cutoff no encontrado");

            return File(result.Content, "text/csv", result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToJson(int id)
        {
            var result = await _exportService.GenerateCutoffJsonAsync(id);
            if (result.Content == null) return NotFound("Cutoff no encontrado");

            return File(result.Content, "application/json", result.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToXml(int id)
        {
            var result = await _exportService.GenerateCutoffXmlAsync(id);
            if (result.Content == null) return NotFound("Cutoff no encontrado");

            return File(result.Content, "application/xml", result.FileName);
        }
        // GET: Cutoffs/Create
        public async Task<IActionResult> Create()
        {
            // Preparar información de user para la vista y site.js
            ViewData["IsAuthenticated"] = User.Identity?.IsAuthenticated ?? false;
            ViewData["UserName"] = User.Identity?.Name ?? string.Empty;
            ViewBag.IsRH = User.HasClaim("UserType", ((int)UserTypeEnum.HumanResources).ToString());
            ViewBag.IsAdministrator = User.HasClaim("UserType", ((int)UserTypeEnum.Administrator).ToString());
            ViewData["UserId"] = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Cierre automático lazy
            await _cutoffService.AutoCloseCutoffIfNeededAsync();

            var viewModel = new CutoffCreateViewModel();

            // Buscar el último cutoff (activo o no) para basar la fecha de inicio
            var lastCutoff = await _cutoffService.GetLastCutoffAsync();
            
            if (lastCutoff != null)
            {
                // El nuevo cutoff inicia el día siguiente al fin del anterior
                viewModel.StartDate = lastCutoff.EndDate.Date.AddDays(1);
            }
            else
            {
                // Si es el primer cutoff del sistema, usamos hoy
                viewModel.StartDate = DateTime.Now.Date;
            }

            // Fecha de término por defecto es el día actual, 
            // pero asegurando que sea al menos igual o posterior al inicio para evitar conflictos inmediatos
            viewModel.EndDate = DateTime.Now.Date > viewModel.StartDate ? DateTime.Now.Date : viewModel.StartDate.AddDays(2);
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CutoffCreateViewModel viewModel)
        {
            // Verificar que el user esté autenticado
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            // Validaciones estándar
            if (viewModel.EndDate <= viewModel.StartDate)
                ModelState.AddModelError("EndDate", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            
            var corteActivo = await _cutoffService.GetActiveCutoffAsync();
            if (corteActivo != null && viewModel.EndDate <= corteActivo.EndDate)
                ModelState.AddModelError("EndDate", "La fecha de fin debe ser posterior al fin del corte actual.");

            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var newCutoff = new Cutoff
                {
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    CutoffStatusTypeId = 1 // Open por defecto
                };

                var success = await _cutoffService.CreateCutoffAsync(newCutoff, userId);
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

        // GET: Cutoffs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cutoff = await _context.Cutoffs.FindAsync(id);
            if (cutoff == null)
            {
                return NotFound();
            }
            ViewData["CreatedByUserId"] = new SelectList(_context.Users, "Id", "Name", cutoff.CreatedByUserId);
            ViewData["UpdatedByUserId"] = new SelectList(_context.Users, "Id", "Name", cutoff.UpdatedByUserId);
            return View(cutoff);
        }

        // POST: Cutoffs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDate,EndDate,CreatedAt,UpdatedAt,CutoffStatusTypeId,CreatedByUserId,UpdatedByUserId")] Cutoff cutoff)
        {
            if (id != cutoff.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    cutoff.UpdatedAt = DateTime.Now; // UpdatedAt remains DateTime
                    _context.Update(cutoff);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cutoffs.Any(e => e.Id == cutoff.Id))
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
                    _logger.LogError(ex, "Error al editar corte {Id}. Detalle: {Inner}", cutoff.Id, inner);
                    ModelState.AddModelError("", $"Error al actualizar el corte: {ex.Message}. Detalle: {inner}");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedByUserId"] = new SelectList(_context.Users, "Id", "Name", cutoff.CreatedByUserId);
            ViewData["UpdatedByUserId"] = new SelectList(_context.Users, "Id", "Name", cutoff.UpdatedByUserId);
            return View(cutoff);
        }

        // GET: Cutoffs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cutoff == null)
            {
                return NotFound();
            }

            return View(cutoff);
        }

        // POST: Cutoff/Delete/5 - borrado lógico
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var success = await _cutoffService.InactivateCutoffAsync(id, userId);
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

            if (!User.HasClaim("UserType", ((int)UserTypeEnum.Administrator).ToString()))
                return Json(new AlertResponseDTO { Success = false, Title = "Acceso denegado", Message = "No tiene permisos para eliminar cortes.", Icon = "error" });

            var cutoff = await _context.Cutoffs.FindAsync(request.Id);
            if (cutoff == null)
                return Json(new AlertResponseDTO { Success = false, Title = "No encontrado", Message = "Cutoff no encontrado.", Icon = "error" });

            if (!request.Confirmed)
            {
                return Json(new AlertResponseDTO
                {
                    ShowConfirmation = true,
                    Title = "¿Cerrar e Inactivar Periodo?",
                    Message = $"¿Estás seguro de que deseas cerrar el periodo que inicia **{cutoff.StartDate.ToString("dd/MM/yyyy")}**?<br><br><small class='text-muted'>Esta acción marcará los permisos asociados como procesados.</small>",
                    Icon = "warning",
                    ConfirmButtonText = "Sí, cerrar periodo",
                    CancelButtonText = "Cancelar"
                });
            }

            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _cutoffService.InactivateCutoffAsync(request.Id, userId);

            if (success)
            {
                return Json(new AlertResponseDTO { Success = true, Title = "Periodo Closed", Message = "El periodo ha sido inactivado y cerrado correctamente.", Icon = "success", RedirectUrl = "/Cutoff/Index" });
            }
            else
            {
                return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "Ocurrió un error al inactivar.", Icon = "error" });
            }
        }


        // AJAX: Close Period via fetch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAjax([FromBody] ActionRequestDTO request)
        {
            if (request == null || request.Id <= 0)
                return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "ID no válido.", Icon = "error" });

            if (!User.HasClaim("UserType", ((int)UserTypeEnum.HumanResources).ToString()) && 
                !User.HasClaim("UserType", ((int)UserTypeEnum.Administrator).ToString()))
            {
                return Json(new AlertResponseDTO { Success = false, Title = "Acceso denegado", Message = "No tiene permisos para cerrar periodos.", Icon = "error" });
            }

            var cutoff = await _context.Cutoffs.FindAsync(request.Id);
            if (cutoff == null)
                return Json(new AlertResponseDTO { Success = false, Title = "No encontrado", Message = "Periodo no encontrado.", Icon = "error" });

            if (cutoff.CutoffStatusTypeId != (int)CutoffStatusEnum.Open)
                return Json(new AlertResponseDTO { Success = false, Title = "Estado inválido", Message = "Solo se pueden cerrar periodos que estén Abiertos.", Icon = "warning" });

            if (!request.Confirmed)
            {
                // Verificar si hay permissions pendientes para advertir
                bool hasPending = await _cutoffService.HasPendingPermissionsAsync(request.Id);
                string extraWarning = hasPending 
                    ? "<br><br><div class='alert alert-danger py-2 small'><i class='fas fa-exclamation-triangle me-1'></i> <strong>¡Atención!</strong> Hay permisos pendientes que serán <strong>RECHAZADOS</strong> automáticamente al cerrar.</div>" 
                    : "";

                return Json(new AlertResponseDTO
                {
                    ShowConfirmation = true,
                    Title = "¿Cerrar Periodo de Cutoff?",
                    Message = $"¿Estás seguro de que deseas finalizar oficialmente el periodo que termina el **{cutoff.EndDate.ToString("dd/MM/yyyy")}**?{extraWarning}",
                    Icon = "warning",
                    ConfirmButtonText = "Sí, cerrar periodo",
                    CancelButtonText = "Cancelar"
                });
            }

            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var success = await _cutoffService.CloseCutoffAsync(request.Id, userId);

            if (success)
            {
                _logger.LogInformation("User {UserId} cerró manualmente el periodo {CutoffId}", userId, request.Id);
                return Json(new AlertResponseDTO { Success = true, Title = "Periodo Finalizado", Message = "El periodo ha sido cerrado y los permisos pendientes han sido procesados.", Icon = "success", RedirectUrl = "/Cutoff/Index" });
            }
            else
            {
                return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "Ocurrió un error al intentar cerrar el periodo.", Icon = "error" });
            }
        }

       [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 1. Obtener el cutoff y sus datos de auditoría
            var cutoff = await _context.Cutoffs
                .Include(c => c.CreatedByUser)
                .Include(c => c.UpdatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cutoff == null) return NotFound();

            // 2. Obtener los permissions activos para las estadísticas
            var permissions = await _context.Permissions
                .Where(p => p.CutoffId == id && p.PermissionStatusTypeId != 3) // Excluyendo Invalidados
                .Include(p => p.PermissionType)
                .Include(p => p.RequestingUser)
                    .ThenInclude(u => u.Area)
                .ToListAsync();

            // 3. Paleta de colores para las gráficas
            string[] palette = new string[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899" };

            // 4. Procesar Datos para JSON (Gráficas)
            var permissionsByArea = permissions
                .Where(p => p.RequestingUser?.Area != null)
                .GroupBy(p => p.RequestingUser!.Area!)
                .Select((g, index) => new {
                    Id = g.Key.Id,
                    Name = g.Key.Description,
                    Value = g.Count(),
                    Color = palette[index % palette.Length]
                }).OrderByDescending(x => x.Value).ToList();

            var permissionsByType = permissions
                .GroupBy(p => p.PermissionType!)
                .Select((g, index) => new {
                    Name = g.Key?.Description ?? "Otros",
                    Value = g.Count(),
                    Color = palette[(index + 2) % palette.Length]
                }).ToList();

            var trend = permissions
                .OrderBy(p => p.CreatedAt)
                .GroupBy(p => p.CreatedAt.ToString("dd MMM"))
                .Select(g => new {
                    Month = g.Key ?? "S/F",
                    Total = g.Count(),
                    Approved = g.Count(x => x.PermissionStatusTypeId == 2),
                    Pending = g.Count(x => x.PermissionStatusTypeId == 1)
                }).ToList();

            var heatmap = permissions
                .GroupBy(p => new { 
                    Day = p.CreatedAt.ToString("dddd", new CultureInfo("en-US")), 
                    Week = (p.CreatedAt.Day / 7) + 1 
                })
                .Select(g => new { Day = g.Key.Day, Week = g.Key.Week, Count = g.Count() })
                .ToList();

            // 5. Mapear al DTO
            var model = new CutoffDetailsDTO
            {
                Id = cutoff.Id,
                StartDate = cutoff.StartDate,
                EndDate = cutoff.EndDate,
                CutoffStatusTypeId = cutoff.CutoffStatusTypeId,
                StatusDescription = cutoff.CutoffStatusType?.Description,
                StatusColor = cutoff.CutoffStatusType?.Color,
                CreatedBy = cutoff.CreatedByUser?.Name ?? "Sistema",
                UpdatedBy = cutoff.UpdatedByUser?.Name,
                TotalPermissions = permissions.Count,
                WithPay = permissions.Count(p => p.WithPay == true),
                WithoutPay = permissions.Count(p => p.WithPay != true),
                AverageDays = permissions.Any() ? permissions.Average(p => p.Days ?? 0) : 0,
                
                // Serialización
                PermissionsByTypeJson = JsonConvert.SerializeObject(permissionsByType),
                PermissionsByAreaJson = JsonConvert.SerializeObject(permissionsByArea),
                TrendJson = JsonConvert.SerializeObject(trend),
                HeatmapDataJson = JsonConvert.SerializeObject(heatmap),
                KpiDataJson = JsonConvert.SerializeObject(new { total = permissions.Count })
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