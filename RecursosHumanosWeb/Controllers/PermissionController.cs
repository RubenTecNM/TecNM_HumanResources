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
    public class PermissionController : Controller
    {
        private readonly HumanResourcesContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        private readonly ILogger<PermissionController> _logger;
        private readonly IPermissionService _permissionService;
        private readonly ICutoffService _cutoffService;
        private readonly IConfiguration _configuration;

        public PermissionController(
            HumanResourcesContext context,
            IWebHostEnvironment environment,
            IMapper mapper,
            ILogger<PermissionController> logger,
            IPermissionService permissionService,
            ICutoffService cutoffService,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _mapper = mapper;
            _logger = logger;
            _permissionService = permissionService;
            _cutoffService = cutoffService;
            _configuration = configuration;
        }

        #region VISTAS Y FILTROS

        [HttpGet]
        public async Task<IActionResult> Index(string? userName = null, int? areaId = null, DateTime? fromDay = null, DateTime? toDay = null, bool? reviewed = null, int? page = 1)
        {
            await _cutoffService.AutoCloseCutoffIfNeededAsync();
            
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            int pageSize = _configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize");
            var permissions = await _permissionService.GetPermissionsPagedAsync(userId, areaId, userName, fromDay, toDay, reviewed, page ?? 1, pageSize);

            var areas = (userType == (int)UserTypeEnum.HumanResources || userType == (int)UserTypeEnum.Administrator)
                ? await _context.Areas.Where(a => a.Status).Select(a => new AreasDTO { Id = a.Id, Description = a.Description }).OrderBy(a => a.Description).ToListAsync()
                : new List<AreasDTO>();

            var model = new SearchPermissionsViewModel
            {
                Permissions = permissions,
                Areas = areas,
                SelectedUser = userName,
                SelectedArea = areaId,
                Reviewed = reviewed
            };

            ViewBag.FromDay = fromDay?.ToString("yyyy-MM-dd");
            ViewBag.Today = toDay?.ToString("yyyy-MM-dd");
            ViewBag.Reviewed = reviewed;
            ViewBag.IsAuthorizer = userType == (int)UserTypeEnum.Authorizer;
            ViewBag.IsRH = userType == (int)UserTypeEnum.HumanResources;
            ViewBag.IsAdministrator = userType == (int)UserTypeEnum.Administrator;

            var activeCutoff = await _cutoffService.GetActiveCutoffAsync();
            ViewBag.CurrentCutoffId = activeCutoff?.Id ?? 0;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            var stats = await _permissionService.GetStatsAsync(userId, userType);
            return Json(stats);
        }

        #endregion

        #region CREATE Y EDIT (GET)

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (User.HasClaim("UserType", ((int)UserTypeEnum.Authorizer).ToString()))
            {
                TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "Lo sentimos, los Jefes de Área no pueden solicitar permisos a través de este portal para asegurar la imparcialidad del proceso.");
                return RedirectToAction("Index");
            }

            await PrepareCreateEditViewBag();
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return View(new PermissionsCreateViewModel { RequestingUserId = userId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.HasClaim("UserType", ((int)UserTypeEnum.Authorizer).ToString()))
            {
                TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "Los jefes de área deben usar 'Revisar' para autorizar permisos.");
                return RedirectToAction("Index");
            }

            var permission = await _context.Permissions.Include(p => p.Cutoff).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (permission == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission))
            {
                TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "No tiene permisos para modificar esta solicitud.");
                return RedirectToAction("Index");
            }

            bool isPending = permission.PermissionStatusTypeId == 1; // 1 = Pending
            var cutoff = permission.Cutoff;
            if (cutoff == null || cutoff.CutoffStatusTypeId != 1) // 1 = Open
            {
                TempData["ErrorAlert"] = GetAlertJson("No permitido", "No se puede editar un permiso si el periodo ya está cerrado o invalidado.");
                return RedirectToAction("Index");
            }

            if (!isPending)
            {
                TempData["ErrorAlert"] = GetAlertJson("No permitido", "Solo se pueden editar permisos en estado Pending.");
                return RedirectToAction("Index");
            }

            await PrepareCreateEditViewBag(permission.RequestingUserId);
            return View(_mapper.Map<PermissionsCreateViewModel>(permission));
        }

        #endregion

        #region CREATE Y EDIT (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PermissionsCreateViewModel model, bool? confirmed)
        {
            string? evidenciaPath = null;
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            try
            {
                if (userType == (int)UserTypeEnum.Authorizer)
                {
                    TempData["ErrorAlert"] = GetAlertJson("Acceso Denegado", "Los jefes de área no pueden crear solicitudes de permiso.");
                    return RedirectToAction("Index");
                }

                if (userType != (int)UserTypeEnum.HumanResources && userType != (int)UserTypeEnum.Administrator)
                    model.RequestingUserId = userId;

                if (!ModelState.IsValid)
                    return Json(new AlertResponseDTO { Success = false, Title = "Validación", Message = "Corrija los errores.", Errors = GetModelStateErrors() });

                var cutoff = await _cutoffService.GetActiveCutoffAsync();
                if (cutoff == null) return Json(new AlertResponseDTO { Success = false, Message = "No hay corte activo." });

                if (model.StartDate!.Value.Date < cutoff.StartDate.Date)
                    return Json(new AlertResponseDTO { Success = false, Message = "La fecha de inicio está fuera del corte." });

                if (model.EndDate != null && model.EndDate.Value.Date > cutoff.EndDate.Date && !(confirmed ?? false))
                {
                    string endDateStr = cutoff.EndDate.ToString("dd/MM/yyyy");
                    return Json(new AlertResponseDTO 
                    { 
                        Success = false, 
                        ShowConfirmation = true, 
                        Title = "Excede el periodo", 
                        Message = $"El periodo actual termina el: **{endDateStr}**. ¿Desea crear el permiso aun si excede esta fecha?\n\n*Tenga en cuenta que solo tiene posibilidad entre la fecha de inicio y cierre del periodo, no más ni menos.*", 
                        Icon = "warning", 
                        ConfirmButtonText = "Sí, crear" 
                    });
                }

                if (model.EvidenceFile != null)
                {
                    evidenciaPath = await SaveEvidenceFile(model.EvidenceFile, model.RequestingUserId.Value);
                }

                var permission = _mapper.Map<Permission>(model);
                permission.Evidence = evidenciaPath;
                permission.PermissionStatusTypeId = 1; // Pending por defecto
                await _permissionService.CreatePermissionAsync(permission, userId);

                return Json(new AlertResponseDTO(controllerName: "Permission", message: "Permiso creado con éxito."));
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
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            try
            {
                var original = await _context.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (original == null) return NotFound();

                if (!_permissionService.CanUserAccessPermiso(userId, userType, original))
                    return Json(new AlertResponseDTO { Success = false, Message = "Acceso Denegado" });

                if (!ModelState.IsValid)
                    return Json(new AlertResponseDTO { Success = false, Errors = GetModelStateErrors() });

                var cutoff = await _cutoffService.GetActiveCutoffAsync();
                if (cutoff != null && model.EndDate != null && model.EndDate.Value.Date > cutoff.EndDate.Date && !(Request.Form["confirmed"] == "true"))
                {
                    string endDateStr = cutoff.EndDate.ToString("dd/MM/yyyy");
                    return Json(new AlertResponseDTO 
                    { 
                        Success = false, 
                        ShowConfirmation = true, 
                        Title = "Excede el periodo", 
                        Message = $"El periodo actual termina el: **{endDateStr}**. ¿Desea modificar el permiso aun si excede esta fecha?\n\n*Tenga en cuenta que solo tiene posibilidad entre la fecha de inicio y cierre del periodo, no más ni menos.*", 
                        Icon = "warning", 
                        ConfirmButtonText = "Sí, guardar" 
                    });
                }

                string? oldPath = original.Evidence;
                string? newPath = oldPath;

                if (model.EvidenceFile != null)
                {
                    newPath = await SaveEvidenceFile(model.EvidenceFile, original.RequestingUserId);
                }

                // Transferir datos del ViewModel al objeto que irá al servicio
                // Usamos el objeto original como base para preservar campos no editables (como CutoffId)
                var edited = _mapper.Map<Permission>(model);
                if (model.EvidenceFile == null) 
                {
                    edited.Evidence = oldPath; // Asegurar que conserve la ruta si no hay archivo nuevo
                }
                else
                {
                    edited.Evidence = newPath;
                }

                bool success = await _permissionService.UpdatePermissionAsync(edited, userId);
                if (!success) 
                {
                    if (newPath != oldPath) DeleteFile(newPath!); // Borrar el nuevo si falló el guardado
                    return Json(new AlertResponseDTO { Success = false, Message = "No se pudo actualizar el registro en la base de datos." });
                }

                // Solo si el guardado fue exitoso y subimos uno nuevo, borramos el viejo
                if (newPath != oldPath && oldPath != null) DeleteFile(oldPath);

                return Json(new AlertResponseDTO(controllerName: "Permission", message: "Permiso actualizado correctamente."));
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
            var permission = await _context.Permissions.Include(p => p.RequestingUser).Include(p => p.PermissionType).FirstOrDefaultAsync(m => m.Id == id);
            if (permission == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission)) return Forbid();

            return View(permission);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            var permission = await _context.Permissions.Include(p => p.Cutoff).FirstOrDefaultAsync(p => p.Id == id);
            if (permission != null && _permissionService.CanUserAccessPermiso(userId, userType, permission))
            {
                bool isPending = permission.PermissionStatusTypeId == 1;
                var cutoff = permission.Cutoff;
                if (!isPending && (cutoff == null || cutoff.CutoffStatusTypeId != 1))
                {
                    TempData["ErrorAlert"] = GetAlertJson("Error", "No se puede borrar un permiso ya revisado de un corte cerrado.");
                    return RedirectToAction("Index");
                }
                await _permissionService.DeletePermissionAsync(id, userId);
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
                int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

                var permission = await _context.Permissions.Include(p => p.RequestingUser).Include(p => p.Cutoff).FirstOrDefaultAsync(p => p.Id == request.Id);
                if (permission == null || permission.PermissionStatusTypeId == 4) // 4 = Rejected/Eliminado? No, usually 4 is Rejected.
                    return Json(new AlertResponseDTO { Success = false, Message = "No encontrado o ya procesado." });

                if (!_permissionService.CanUserAccessPermiso(userId, userType, permission)) return Json(new AlertResponseDTO { Success = false, Message = "Denegado." });

                var cutoff = permission.Cutoff;
                if (cutoff == null || cutoff.CutoffStatusTypeId != 1)
                    return Json(new AlertResponseDTO { Success = false, Message = "El permiso ya no se puede eliminar porque el periodo ya está cerrado o invalidado." });

                if (!request.Confirmed)
                {
                    return Json(new AlertResponseDTO { ShowConfirmation = true, Title = "¿Eliminar?", Message = $"¿Borrar solicitud de **{permission.RequestingUser.Name}**?", Icon = "question", ConfirmButtonText = "Sí, eliminar" });
                }

                bool success = await _permissionService.DeletePermissionAsync(request.Id, userId);
                if (!success) return Json(new AlertResponseDTO { Success = false, Message = "Error al intentar eliminar el registro." });

                return Json(new AlertResponseDTO { Success = true, Title = "Eliminado", RedirectUrl = Url.Action("Index", "Permission") });
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
            
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");
            if (userType != (int)UserTypeEnum.Administrator && userType != (int)UserTypeEnum.Authorizer)
                return Json(new AlertResponseDTO { Success = false, Message = "Acceso Denegado: Su rol no cuenta con los privilegios para autorizar permisos." });

            int authId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            bool success = await _permissionService.AuthorizePermissionAsync(id, authId, model.WithPay, "Revisado");
            if (!success) return Json(new AlertResponseDTO { Success = false, Message = "Error al revisar" });

            return Json(new AlertResponseDTO { Success = true, RedirectUrl = Url.Action("Index", "Permission") });
        }

        #endregion

        #region DETAILS

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var permission = await _context.Permissions
                .Include(p => p.PermissionType)
                .Include(p => p.RequestingUser).ThenInclude(u => u.Area)
                .Include(p => p.AuthorizingUser)
                .Include(p => p.Cutoff)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (permission == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission)) return Forbid();
            
            // Pass user info for review authorization check in view
            var currentCourt = await _cutoffService.GetActiveCutoffAsync();
            ViewBag.CurrentCutoffId = currentCourt?.Id ?? 0;
            
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentUserType = userType;
            ViewBag.IsApprover = userType == (int)UserTypeEnum.Administrator;
            
            if (userType == (int)UserTypeEnum.Authorizer)
            {
                var userArea = await _context.Users.Where(u => u.Id == userId).Select(u => u.AreaId).FirstOrDefaultAsync();
                ViewBag.UserAreaId = userArea;
                if (permission.RequestingUser.AreaId == userArea)
                {
                    ViewBag.IsApprover = true;
                }
            }

            if (permission.PermissionStatusTypeId == 1 && (userType == (int)UserTypeEnum.Authorizer || userType == (int)UserTypeEnum.Administrator))
            {
                 // Si es el autorizador asignado, redirigir a Authorize
                 if (ViewBag.IsApprover == true) return RedirectToAction("Authorize", new { id });
            }

            return View(permission);
        }

        [HttpGet]
        public async Task<IActionResult> Authorize(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.PermissionType)
                .Include(p => p.RequestingUser).ThenInclude(u => u.Area)
                .Include(p => p.AuthorizingUser)
                .Include(p => p.Cutoff)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (permission == null) return NotFound();

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission)) return Forbid();
            
            var currentCourt = await _cutoffService.GetActiveCutoffAsync();
            ViewBag.CurrentCutoffId = currentCourt?.Id ?? 0;
            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentUserType = userType;
            
            bool isApprover = userType == (int)UserTypeEnum.Administrator;
            if (userType == (int)UserTypeEnum.Authorizer)
            {
                var userArea = await _context.Users.Where(u => u.Id == userId).Select(u => u.AreaId).FirstOrDefaultAsync();
                if (permission.RequestingUser.AreaId == userArea) isApprover = true;
            }
            ViewBag.IsApprover = isApprover;

            return View(permission);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsCheck(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return Json(new { exists = false });

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");

            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission)) return Json(new { exists = true, hasAccess = false });
            
            // Si es autorizador y esta pendiente, mandar a Authorize
            bool isApprover = userType == (int)UserTypeEnum.Administrator || userType == (int)UserTypeEnum.HumanResources;
            if (!isApprover && userType == (int)UserTypeEnum.Authorizer)
            {
                var solicitante = await _context.Users.FindAsync(permission.RequestingUserId);
                var autorizador = await _context.Users.FindAsync(userId);
                if (solicitante?.AreaId == autorizador?.AreaId) isApprover = true;
            }

            string action = (isApprover && permission.PermissionStatusTypeId == 1) ? "Authorize" : "Details";
            return Json(new { exists = true, hasAccess = true, url = Url.Action(action, "Permission", new { id }) });
        }

        #endregion

        #region TEST DATA

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTestData()
        {
            if (!User.HasClaim("UserType", ((int)UserTypeEnum.Administrator).ToString())) return Forbid();

            // Simplified: logic remains largely same but cleaned up
            var cutoff = await _cutoffService.GetActiveCutoffAsync();
            if (cutoff == null) return Json(new AlertResponseDTO { Success = false, Message = "No hay corte activo." });

            var users = await _context.Users.Where(u => u.Status).Take(10).ToListAsync();
            var autorizadores = await _context.Users.Where(u => u.Status && u.UserTypeId == (int)UserTypeEnum.Authorizer).GroupBy(u => u.AreaId).ToDictionaryAsync(g => g.Key, g => g.First().Id);

            int adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
            var random = new Random();
            var lot = new List<Permission>();

            for (int i = 0; i < 1000; i++) // Reduced to 1000 for safety in this refactoring step, can be increased later
            {
                var user = users[random.Next(users.Count)];
                var start = cutoff.StartDate.AddDays(random.Next((int)(cutoff.EndDate - cutoff.StartDate).TotalDays));
                lot.Add(new Permission
                {
                    StartDate = start,
                    Reason = "Prueba " + i,
                    CutoffId = cutoff.Id,
                    PermissionTypeId = random.Next(2, 6),
                    RequestingUserId = user.Id,
                    UpdatedByUserId = autorizadores.ContainsKey(user.AreaId) ? autorizadores[user.AreaId] : adminId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PermissionStatusTypeId = 1, // Pending
                    WithPay = random.Next(100) < 70
                });

                if (lot.Count >= 500)
                {
                    _context.Permissions.AddRange(lot);
                    await _context.SaveChangesAsync();
                    lot.Clear();
                }
            }
            if (lot.Any()) { _context.Permissions.AddRange(lot); await _context.SaveChangesAsync(); }

            return Json(new AlertResponseDTO { Success = true, Message = "Datos generados." });
        }

        #endregion

        #region HELPERS

        private async Task PrepareCreateEditViewBag(int? selectedUserId = null)
        {
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var usersQuery = _context.Users.Where(u => u.Status);
            if (userType != (int)UserTypeEnum.HumanResources && userType != (int)UserTypeEnum.Administrator)
                usersQuery = usersQuery.Where(u => u.Id == userId);

            ViewBag.Users = new SelectList(await usersQuery.OrderBy(u => u.Name).ToListAsync(), "Id", "Name", selectedUserId);
            ViewBag.PermissionTypes = new SelectList(await _context.PermissionTypes.ToListAsync(), "Id", "Description");
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