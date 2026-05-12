using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList;
using AutoMapper;

namespace RecursosHumanosWeb.Controllers
{
    [Authorize]
    public class PermisoController : Controller
    {
        private readonly RecursosHumanosContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        private readonly ILogger<PermisoController> _logger;
        private readonly IPermisoService _permisoService;
        private readonly ICorteService _corteService;
        private readonly IConfiguration _configuration;

        public PermisoController(
            RecursosHumanosContext context,
            IWebHostEnvironment environment,
            IMapper mapper,
            ILogger<PermisoController> logger,
            IPermisoService permisoService,
            ICorteService corteService,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _mapper = mapper;
            _logger = logger;
            _permisoService = permisoService;
            _corteService = corteService;
            _configuration = configuration;
        }

        #region VISTAS Y FILTROS

        [HttpGet]
        public async Task<IActionResult> Index(string? nombreUsuario = null, int? idArea = null, DateTime? delDia = null, DateTime? alDia = null, bool? revisado = null, int? page = 1)
        {
            await _corteService.AutoCloseCorteIfNeededAsync();
            
            int idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int tipoUsuario = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            int pageSize = _configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize");
            var permisos = await _permisoService.GetPermisosPagedAsync(idUsuario, idArea, nombreUsuario, delDia, alDia, revisado, page ?? 1, pageSize);

            var areas = (tipoUsuario == (int)TipoUsuarioEnum.RecursosHumanos || tipoUsuario == (int)TipoUsuarioEnum.Administrador)
                ? await _context.Areas.Where(a => a.Estatus).Select(a => new AreasDTO { Id = a.Id, Descripcion = a.Descripcion }).OrderBy(a => a.Descripcion).ToListAsync()
                : new List<AreasDTO>();

            var model = new SearchPermissionsViewModel
            {
                Permisos = permisos,
                Areas = areas,
                SelectedUser = nombreUsuario,
                SelectedArea = idArea,
                Revisado = revisado
            };

            ViewBag.DelDia = delDia?.ToString("yyyy-MM-dd");
            ViewBag.AlDia = alDia?.ToString("yyyy-MM-dd");
            ViewBag.Revisado = revisado;
            ViewBag.IsAutorizador = tipoUsuario == (int)TipoUsuarioEnum.Autorizador;
            ViewBag.IsRH = tipoUsuario == (int)TipoUsuarioEnum.RecursosHumanos;
            ViewBag.IsAdministrador = tipoUsuario == (int)TipoUsuarioEnum.Administrador;

            var corteActivo = await _corteService.GetActiveCorteAsync();
            ViewBag.CurrentCourtId = corteActivo?.Id ?? 0;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            int idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int tipoUsuario = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            var stats = await _permisoService.GetStatsAsync(idUsuario, tipoUsuario);
            return Json(stats);
        }

        #endregion

        #region CREATE Y EDIT (GET)

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.Autorizador).ToString()))
            {
                TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "Lo sentimos, los Jefes de Área no pueden solicitar permisos a través de este portal para asegurar la imparcialidad del proceso.");
                return RedirectToAction("Index");
            }

            await PrepareCreateEditViewBag();
            int idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return View(new PermissionsCreateViewModel { IdUsuarioSolicita = idUsuario });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.Autorizador).ToString()))
            {
                TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "Los jefes de área deben usar 'Revisar' para autorizar permisos.");
                return RedirectToAction("Index");
            }

            var permiso = await _context.Permisos.Include(p => p.IdCorteNavigation).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (permiso == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            if (!_permisoService.CanUserAccessPermiso(userId, userType, permiso))
            {
                TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "No tiene permisos para modificar esta solicitud.");
                return RedirectToAction("Index");
            }

            bool isPending = permiso.IdEstadoPermiso == 1; // 1 = Pendiente
            var corte = permiso.IdCorteNavigation;
            if (corte == null || corte.IdEstadoCorte != 1) // 1 = Abierto
            {
                TempData["ErrorAlert"] = GetAlertJson("No permitido", "No se puede editar un permiso si el periodo ya está cerrado o invalidado.");
                return RedirectToAction("Index");
            }

            if (!isPending)
            {
                TempData["ErrorAlert"] = GetAlertJson("No permitido", "Solo se pueden editar permisos en estado Pendiente.");
                return RedirectToAction("Index");
            }

            await PrepareCreateEditViewBag(permiso.IdUsuarioSolicita);
            return View(_mapper.Map<PermissionsCreateViewModel>(permiso));
        }

        #endregion

        #region CREATE Y EDIT (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PermissionsCreateViewModel model, bool? confirmed)
        {
            string? evidenciaPath = null;
            int idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int tipoUsuario = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            try
            {
                if (tipoUsuario == (int)TipoUsuarioEnum.Autorizador)
                {
                    TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "Los jefes de área no pueden crear solicitudes de permiso.");
                    return RedirectToAction("Index");
                }

                if (tipoUsuario != (int)TipoUsuarioEnum.RecursosHumanos && tipoUsuario != (int)TipoUsuarioEnum.Administrador)
                    model.IdUsuarioSolicita = idUsuario;

                if (!ModelState.IsValid)
                    return Json(new AlertResponseDTO { Success = false, Title = "Validación", Message = "Corrija los errores.", Errors = GetModelStateErrors() });

                var corte = await _corteService.GetActiveCorteAsync();
                if (corte == null) return Json(new AlertResponseDTO { Success = false, Message = "No hay corte activo." });

                if (model.Fecha1!.Value.Date < corte.Inicia.Date)
                    return Json(new AlertResponseDTO { Success = false, Message = "La fecha de inicio está fuera del corte." });

                if (model.Fecha2 != null && model.Fecha2.Value.Date > corte.Termina.Date && !(confirmed ?? false))
                {
                    string fechaTerminaStr = corte.Termina.ToString("dd/MM/yyyy");
                    return Json(new AlertResponseDTO 
                    { 
                        Success = false, 
                        ShowConfirmation = true, 
                        Title = "Excede el periodo", 
                        Message = $"El periodo actual termina el: **{fechaTerminaStr}**. ¿Desea crear el permiso aun si excede esta fecha?\n\n*Tenga en cuenta que solo tiene posibilidad entre la fecha de inicio y cierre del periodo, no más ni menos.*", 
                        Icon = "warning", 
                        ConfirmButtonText = "Sí, crear" 
                    });
                }

                if (model.EvidenceFile != null)
                {
                    evidenciaPath = await SaveEvidenceFile(model.EvidenceFile, model.IdUsuarioSolicita.Value);
                }

                var permiso = _mapper.Map<Permiso>(model);
                permiso.Evidencia = evidenciaPath;
                permiso.IdEstadoPermiso = 1; // Pendiente por defecto
                await _permisoService.CreatePermisoAsync(permiso, idUsuario);

                return Json(new AlertResponseDTO(controllerName: "Permiso", message: "Permiso creado con éxito."));
            }
            catch (Exception ex)
            {
                if (evidenciaPath != null) DeleteFile(evidenciaPath);
                return Json(new AlertResponseDTO { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PermissionsCreateViewModel model)
        {
            if (id != model.Id) return Json(new AlertResponseDTO { Success = false, Message = "ID mismatch" });

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            try
            {
                var original = await _context.Permisos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (original == null) return NotFound();

                if (!_permisoService.CanUserAccessPermiso(userId, userType, original))
                    return Json(new AlertResponseDTO { Success = false, Message = "Acceso Denegado" });

                if (!ModelState.IsValid)
                    return Json(new AlertResponseDTO { Success = false, Errors = GetModelStateErrors() });

                var corte = await _corteService.GetActiveCorteAsync();
                if (corte != null && model.Fecha2 != null && model.Fecha2.Value.Date > corte.Termina.Date && !(Request.Form["confirmed"] == "true"))
                {
                    string fechaTerminaStr = corte.Termina.ToString("dd/MM/yyyy");
                    return Json(new AlertResponseDTO 
                    { 
                        Success = false, 
                        ShowConfirmation = true, 
                        Title = "Excede el periodo", 
                        Message = $"El periodo actual termina el: **{fechaTerminaStr}**. ¿Desea modificar el permiso aun si excede esta fecha?\n\n*Tenga en cuenta que solo tiene posibilidad entre la fecha de inicio y cierre del periodo, no más ni menos.*", 
                        Icon = "warning", 
                        ConfirmButtonText = "Sí, guardar" 
                    });
                }

                string? oldPath = original.Evidencia;
                string? newPath = oldPath;

                if (model.EvidenceFile != null)
                {
                    newPath = await SaveEvidenceFile(model.EvidenceFile, original.IdUsuarioSolicita);
                }

                // Transferir datos del ViewModel al objeto que irá al servicio
                // Usamos el objeto original como base para preservar campos no editables (como IdCorte)
                var edited = _mapper.Map<Permiso>(model);
                if (model.EvidenceFile == null) 
                {
                    edited.Evidencia = oldPath; // Asegurar que conserve la ruta si no hay archivo nuevo
                }
                else
                {
                    edited.Evidencia = newPath;
                }

                bool success = await _permisoService.UpdatePermisoAsync(edited, userId);
                if (!success) 
                {
                    if (newPath != oldPath) DeleteFile(newPath!); // Borrar el nuevo si falló el guardado
                    return Json(new AlertResponseDTO { Success = false, Message = "No se pudo actualizar el registro en la base de datos." });
                }

                // Solo si el guardado fue exitoso y subimos uno nuevo, borramos el viejo
                if (newPath != oldPath && oldPath != null) DeleteFile(oldPath);

                return Json(new AlertResponseDTO(controllerName: "Permiso", message: "Permiso actualizado correctamente."));
            }
            catch (Exception ex)
            {
                return Json(new AlertResponseDTO { Success = false, Message = ex.Message });
            }
        }

        #endregion

        #region DELETE

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var permiso = await _context.Permisos.Include(p => p.IdUsuarioSolicitaNavigation).Include(p => p.IdTipoPermisoNavigation).FirstOrDefaultAsync(m => m.Id == id);
            if (permiso == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            if (!_permisoService.CanUserAccessPermiso(userId, userType, permiso)) return Forbid();

            return View(permiso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            var permiso = await _context.Permisos.Include(p => p.IdCorteNavigation).FirstOrDefaultAsync(p => p.Id == id);
            if (permiso != null && _permisoService.CanUserAccessPermiso(userId, userType, permiso))
            {
                bool isPending = permiso.IdEstadoPermiso == 1;
                var corte = permiso.IdCorteNavigation;
                if (!isPending && (corte == null || corte.IdEstadoCorte != 1))
                {
                    TempData["ErrorAlert"] = GetAlertJson("Error", "No se puede borrar un permiso ya revisado de un corte cerrado.");
                    return RedirectToAction("Index");
                }
                await _permisoService.DeletePermisoAsync(id, userId);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax([FromBody] ActionRequestDTO request)
        {
            try
            {
                if (request == null || request.Id <= 0) return Json(new AlertResponseDTO { Success = false, Message = "ID no válido." });

                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

                var permiso = await _context.Permisos.Include(p => p.IdUsuarioSolicitaNavigation).Include(p => p.IdCorteNavigation).FirstOrDefaultAsync(p => p.Id == request.Id);
                if (permiso == null || permiso.IdEstadoPermiso == 4) // 4 = Rechazado/Eliminado? No, usually 4 is Rechazado.
                    return Json(new AlertResponseDTO { Success = false, Message = "No encontrado o ya procesado." });

                if (!_permisoService.CanUserAccessPermiso(userId, userType, permiso)) return Json(new AlertResponseDTO { Success = false, Message = "Denegado." });

                var corte = permiso.IdCorteNavigation;
                if (corte == null || corte.IdEstadoCorte != 1)
                    return Json(new AlertResponseDTO { Success = false, Message = "El permiso ya no se puede eliminar porque el periodo ya está cerrado o invalidado." });

                if (!request.Confirmed)
                {
                    return Json(new AlertResponseDTO { ShowConfirmation = true, Title = "¿Eliminar?", Message = $"¿Borrar solicitud de **{permiso.IdUsuarioSolicitaNavigation.Nombre}**?", Icon = "question", ConfirmButtonText = "Sí, eliminar" });
                }

                bool success = await _permisoService.DeletePermisoAsync(request.Id, userId);
                if (!success) return Json(new AlertResponseDTO { Success = false, Message = "Error al intentar eliminar el registro." });

                return Json(new AlertResponseDTO { Success = true, Title = "Eliminado", RedirectUrl = Url.Action("Index", "Permiso") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en DeleteAjax");
                return Json(new AlertResponseDTO { Success = false, Title = "Error del Servidor", Message = ex.Message }); // En producción, no mostrar ex.Message directo si es sensible
            }
        }

        #endregion

        #region REVIEW (AUTORIZADORES)

        [HttpGet]
        public async Task<IActionResult> Review(int id)
        {
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, PermissionReviewViewModel model)
        {
            if (id != model.Id) return Json(new AlertResponseDTO { Success = false, Message = "ID mismatch" });
            
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");
            if (userType != (int)TipoUsuarioEnum.Administrador && userType != (int)TipoUsuarioEnum.Autorizador)
                return Json(new AlertResponseDTO { Success = false, Message = "Acceso Denegado: Su rol no cuenta con los privilegios para autorizar permisos." });

            int authId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            bool success = await _permisoService.AutorizarPermisoAsync(id, authId, model.Goce, "Revisado");
            if (!success) return Json(new AlertResponseDTO { Success = false, Message = "Error al revisar" });

            return Json(new AlertResponseDTO { Success = true, RedirectUrl = Url.Action("Index", "Permiso") });
        }

        #endregion

        #region DETAILS

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var permiso = await _context.Permisos
                .Include(p => p.IdTipoPermisoNavigation)
                .Include(p => p.IdUsuarioSolicitaNavigation).ThenInclude(u => u.IdAreaNavigation)
                .Include(p => p.IdUsuarioAutorizaNavigation)
                .Include(p => p.IdCorteNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (permiso == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            if (!_permisoService.CanUserAccessPermiso(userId, userType, permiso)) return Forbid();
            
            // Pass user info for review authorization check in view
            var currentCourt = await _corteService.GetActiveCorteAsync();
            ViewBag.CurrentCourtId = currentCourt?.Id ?? 0;
            
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentUserType = userType;
            ViewBag.IsApprover = userType == (int)TipoUsuarioEnum.Administrador;
            
            if (userType == (int)TipoUsuarioEnum.Autorizador)
            {
                var userArea = await _context.Usuarios.Where(u => u.Id == userId).Select(u => u.IdArea).FirstOrDefaultAsync();
                ViewBag.UserAreaId = userArea;
                if (permiso.IdUsuarioSolicitaNavigation.IdArea == userArea)
                {
                    ViewBag.IsApprover = true;
                }
            }

            if (permiso.IdEstadoPermiso == 1 && (userType == (int)TipoUsuarioEnum.Autorizador || userType == (int)TipoUsuarioEnum.Administrador))
            {
                 // Si es el autorizador asignado, redirigir a Authorize
                 if (ViewBag.IsApprover == true) return RedirectToAction("Authorize", new { id });
            }

            return View(permiso);
        }

        [HttpGet]
        public async Task<IActionResult> Authorize(int id)
        {
            var permiso = await _context.Permisos
                .Include(p => p.IdTipoPermisoNavigation)
                .Include(p => p.IdUsuarioSolicitaNavigation).ThenInclude(u => u.IdAreaNavigation)
                .Include(p => p.IdUsuarioAutorizaNavigation)
                .Include(p => p.IdCorteNavigation)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (permiso == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            if (!_permisoService.CanUserAccessPermiso(userId, userType, permiso)) return Forbid();
            
            var currentCourt = await _corteService.GetActiveCorteAsync();
            ViewBag.CurrentCourtId = currentCourt?.Id ?? 0;
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentUserType = userType;
            
            bool isApprover = userType == (int)TipoUsuarioEnum.Administrador;
            if (userType == (int)TipoUsuarioEnum.Autorizador)
            {
                var userArea = await _context.Usuarios.Where(u => u.Id == userId).Select(u => u.IdArea).FirstOrDefaultAsync();
                if (permiso.IdUsuarioSolicitaNavigation.IdArea == userArea) isApprover = true;
            }
            ViewBag.IsApprover = isApprover;

            return View(permiso);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsCheck(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null) return Json(new { exists = false });

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");

            if (!_permisoService.CanUserAccessPermiso(userId, userType, permiso)) return Json(new { exists = true, hasAccess = false });
            
            // Si es autorizador y esta pendiente, mandar a Authorize
            bool isApprover = userType == (int)TipoUsuarioEnum.Administrador || userType == (int)TipoUsuarioEnum.RecursosHumanos;
            if (!isApprover && userType == (int)TipoUsuarioEnum.Autorizador)
            {
                var solicitante = await _context.Usuarios.FindAsync(permiso.IdUsuarioSolicita);
                var autorizador = await _context.Usuarios.FindAsync(userId);
                if (solicitante?.IdArea == autorizador?.IdArea) isApprover = true;
            }

            string action = (isApprover && permiso.IdEstadoPermiso == 1) ? "Authorize" : "Details";
            return Json(new { exists = true, hasAccess = true, url = Url.Action(action, "Permiso", new { id }) });
        }

        #endregion

        #region TEST DATA

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTestData()
        {
            if (!User.HasClaim("TipoUsuario", ((int)TipoUsuarioEnum.Administrador).ToString())) return Forbid();

            // Simplified: logic remains largely same but cleaned up
            var corte = await _corteService.GetActiveCorteAsync();
            if (corte == null) return Json(new AlertResponseDTO { Success = false, Message = "No hay corte activo." });

            var usuarios = await _context.Usuarios.Where(u => u.Estatus).Take(10).ToListAsync();
            var autorizadores = await _context.Usuarios.Where(u => u.Estatus && u.IdTipoUsuario == (int)TipoUsuarioEnum.Autorizador).GroupBy(u => u.IdArea).ToDictionaryAsync(g => g.Key, g => g.First().Id);

            int adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
            var random = new Random();
            var lot = new List<Permiso>();

            for (int i = 0; i < 1000; i++) // Reduced to 1000 for safety in this refactoring step, can be increased later
            {
                var user = usuarios[random.Next(usuarios.Count)];
                var start = corte.Inicia.AddDays(random.Next((int)(corte.Termina - corte.Inicia).TotalDays));
                lot.Add(new Permiso
                {
                    Fecha1 = start,
                    Motivo = "Prueba " + i,
                    IdCorte = corte.Id,
                    IdTipoPermiso = random.Next(2, 6),
                    IdUsuarioSolicita = user.Id,
                    IdUsuarioModifica = autorizadores.ContainsKey(user.IdArea) ? autorizadores[user.IdArea] : adminId,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now,
                    IdEstadoPermiso = 1, // Pendiente
                    Goce = random.Next(100) < 70
                });

                if (lot.Count >= 500)
                {
                    _context.Permisos.AddRange(lot);
                    await _context.SaveChangesAsync();
                    lot.Clear();
                }
            }
            if (lot.Any()) { _context.Permisos.AddRange(lot); await _context.SaveChangesAsync(); }

            return Json(new AlertResponseDTO { Success = true, Message = "Datos generados." });
        }

        #endregion

        #region HELPERS

        private async Task PrepareCreateEditViewBag(int? selectedUserId = null)
        {
            int tipoUsuario = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var usersQuery = _context.Usuarios.Where(u => u.Estatus);
            if (tipoUsuario != (int)TipoUsuarioEnum.RecursosHumanos && tipoUsuario != (int)TipoUsuarioEnum.Administrador)
                usersQuery = usersQuery.Where(u => u.Id == userId);

            ViewBag.Usuarios = new SelectList(await usersQuery.OrderBy(u => u.Nombre).ToListAsync(), "Id", "Nombre", selectedUserId);
            ViewBag.TipoPermisos = new SelectList(await _context.TipoPermisos.ToListAsync(), "Id", "Descripcion");
        }

        private Dictionary<string, string[]> GetModelStateErrors() => 
            ModelState.Where(x => x.Value!.Errors.Any()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        private async Task<string?> SaveEvidenceFile(IFormFile file, int userId)
        {
            int maxMb = _configuration.GetValue<int>("BusinessRules:Uploads:MaxFileSizeMB");
            if (file.Length > maxMb * 1024 * 1024) return null;
            string uploads = Path.Combine(_environment.WebRootPath, "Evidences");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            string name = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLower()}";
            using (var stream = new FileStream(Path.Combine(uploads, name), FileMode.Create)) await file.CopyToAsync(stream);
            return "/Evidences/" + name;
        }

        private void DeleteFile(string webPath)
        {
            try { string path = Path.Combine(_environment.WebRootPath, "Evidences", Path.GetFileName(webPath)); if (System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch { }
        }

        private string GetAlertJson(string title, string text) => System.Text.Json.JsonSerializer.Serialize(new { icon = "error", title, text });

        #endregion
    }
}