using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Filters;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.Enums;
using HumanResourcesProduction.Models.ViewModels.Permissions;
using HumanResourcesProduction.Services;

namespace HumanResourcesProduction.Features.Permissions
{
    [Route("api/Permissions")]
    [ApiController]
    public class PermissionsApiController : ControllerBase
    {
        private readonly HumanResourcesContext _context;
        private readonly IMapper _mapper;
        private readonly IEvidenceStorage _evidenceStorage;
        private readonly IConfiguration _configuration;
        private readonly IPermissionService _permissionService;
        private readonly ICutoffService _cutoffService;

        public PermissionsApiController(
            HumanResourcesContext context,
            IMapper mapper,
            IEvidenceStorage evidenceStorage,
            IConfiguration configuration,
            IPermissionService permissionService,
            ICutoffService cutoffService)
        {
            _context = context;
            _mapper = mapper;
            _evidenceStorage = evidenceStorage;
            _configuration = configuration;
            _permissionService = permissionService;
            _cutoffService = cutoffService;
        }

        // GET: api/Permissions
        [HttpGet]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPermissions(
            int page = 1,
            int pageSize = 10,
            int? statusId = null,
            string? userName = null,
            DateTime? fromDay = null,
            DateTime? toDay = null,
            int? areaId = null)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;

            // Scoping por rol + paginado en SQL (reglas de negocio de PermissionService).
            // Admin/RH ven todo; Autorizador ve lo propio + pendientes de su área; Empleado solo lo propio.
            var paged = await _permissionService.GetPermissionsPagedAsync(userId, areaId, userName, fromDay, toDay, statusId, page, pageSize);

            var items = paged.Select(p => new
            {
                p.Id,
                Requester = p.Name,
                p.RequestingUserId,
                Type = p.PermissionType,
                p.PermissionTypeId,
                p.StartDate,
                p.EndDate,
                p.Reason,
                p.PermissionStatusTypeId,
                p.StatusDescription,
                p.StatusColor,
                p.WithPay,
                p.CreatedAt,
                p.UpdatedAt,
                p.Shift,
                p.HoursInClassroom,
                p.HoursDischarge
            }).ToList();

            int totalItems = paged.TotalItemCount;
            int totalPages = paged.PageCount;
            int firstItemOnPage = totalItems == 0 ? 0 : (page - 1) * pageSize + 1;
            int lastItemOnPage = Math.Min(page * pageSize, totalItems);

            return Ok(new
            {
                items,
                page,
                pageSize,
                totalItems,
                totalPages,
                firstItemOnPage,
                lastItemOnPage
            });
        }

        // GET: api/Permissions/stats  (KPIs del panel; se declara antes de {id} para que el literal tenga precedencia)
        [HttpGet("stats")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPermissionsStats()
        {
            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var userType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            var stats = await _permissionService.GetStatsAsync(userId, userType);
            return Ok(stats);
        }


// GET: api/Permissions/create-config  (configuración para el formulario de creación)
[HttpGet("create-config")]
[ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
public async Task<IActionResult> GetCreateConfig()
{
    var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
    var userType = await _context.Users.AsNoTracking()
        .Where(u => u.Id == userId)
        .Select(u => u.UserTypeId)
        .FirstOrDefaultAsync();

    var config = await _permissionService.GetCreateConfigurationAsync(userId, userType);
    return Ok(config);
}

        // GET: api/Permissions/5/evidence  (descarga del PDF adjunto)
        [HttpGet("{id}/evidence")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult GetEvidence(int id)
        {
            var permission = _context.Permissions.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (permission == null || string.IsNullOrWhiteSpace(permission.Evidence)) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var userType = _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefault();

            // Regla de negocio: acceso a la evidencia (misma política que el detalle)
            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission))
                return StatusCode(403, new { error = "Forbidden", message = "No tiene acceso a la evidencia de este permiso." });

            var path = _evidenceStorage.ResolvePhysicalPath(permission.Evidence);
            if (path == null || !System.IO.File.Exists(path)) return NotFound();

            return PhysicalFile(path, "application/pdf", Path.GetFileName(path));
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Leer")]
        public async Task<IActionResult> GetPermission(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.RequestingUser)
                .Include(p => p.PermissionType)
                .Include(p => p.PermissionStatusType)
                .Include(p => p.Cutoff)
                .Include(p => p.AuthorizingUser)
                .Include(p => p.ShiftType)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null) return NotFound($"Permission con ID {id} no encontrado.");

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var userType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            // Regla de negocio: solo quien puede verlo (Admin/RH, el solicitante, o su jefe de área)
            if (!_permissionService.CanUserAccessPermiso(userId, userType, permission))
                return StatusCode(403, new { error = "Forbidden", message = "No tiene acceso para ver este permiso." });

            // Mapeo manual rápido o DTO
            var result = new
            {
                permission.Id,
                Requester = permission.RequestingUser.Name,
                permission.RequestingUserId,
                Type = permission.PermissionType.Description,
                permission.PermissionTypeId,
                permission.StartDate,
                permission.EndDate,
                permission.Days,
                permission.Reason,
                permission.Evidence, // URL relativa
                Status = permission.PermissionStatusTypeId,
                StatusDescription = permission.PermissionStatusType.Description,
                StatusColor = permission.PermissionStatusType.Color ?? "secondary",
                permission.WithPay,
                permission.CreatedAt,
                permission.AuthorizedAt,
                AuthorizedBy = permission.AuthorizingUser?.Name,
                permission.AuthorizationComment,
                Cutoff = permission.Cutoff.StartDate.ToString("yyyy-MM-dd") + " - " + permission.Cutoff.EndDate.ToString("yyyy-MM-dd"),
                permission.Shift,
                ShiftDescription = permission.ShiftType?.Description,
                permission.HoursInClassroom,
                permission.HoursDischarge
            };

            return Ok(result);
        }

        // POST: api/Permissions
        [HttpPost]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Escribir")]
        public async Task<IActionResult> PostPermission([FromForm] PermissionApiCreateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var actorUserType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            // Regla de negocio: los jefes de área no pueden crear solicitudes (imparcialidad del proceso)
            if (actorUserType == (int)UserTypeEnum.Authorizer)
                return StatusCode(403, new { error = "Forbidden", message = "Los jefes de área no pueden crear solicitudes de permiso para asegurar la imparcialidad del proceso." });

            // Asignar solicitante: si viene en el modelo y es válido, usarlo. Si no, usar el user de la API Key.
            int requesterId = model.RequestingUserId ?? userId;

            // Solo Admin/RH pueden solicitar en nombre de otro usuario
            if (model.RequestingUserId.HasValue && model.RequestingUserId.Value != userId &&
                actorUserType != (int)UserTypeEnum.Administrator && actorUserType != (int)UserTypeEnum.HumanResources)
            {
                return StatusCode(403, new { error = "Forbidden", message = "Solo los administradores o RH pueden solicitar permisos para otros usuarios." });
            }

            // Validar archivo de evidencia (PDF, ≤5MB) si se adjuntó
            string? evidencePath = null;
            if (model.EvidenceFile != null && model.EvidenceFile.Length > 0)
            {
                var evidenceError = ValidateEvidenceFile(model.EvidenceFile);
                if (evidenceError != null)
                    return BadRequest(new { success = false, title = "Validación", message = evidenceError });

                evidencePath = await _evidenceStorage.SaveAsync(model.EvidenceFile, requesterId);
            }

            // Encontrar Cutoff activo
            var activeCutoff = await _cutoffService.GetActiveCutoffAsync();

            if (activeCutoff == null)
            {
                if (evidencePath != null) _evidenceStorage.Delete(evidencePath);
                return BadRequest(new
                {
                    success = false,
                    title = "Periodo no disponible",
                    message = "Actualmente no hay un periodo activo para registrar permisos. Recursos Humanos está definiendo el nuevo corte; intenta de nuevo más tarde."
                });
            }

            // Validar que la fecha de inicio esté dentro del corte
            if (model.StartDate.Date < activeCutoff.StartDate.Date)
            {
                if (evidencePath != null) _evidenceStorage.Delete(evidencePath);
                return BadRequest(new { success = false, title = "Fuera del corte", message = "La fecha de inicio está fuera del corte activo." });
            }

            // Si el fin excede el corte, pedir confirmación (flujo en dos pasos)
            if (model.EndDate.HasValue && model.EndDate.Value.Date > activeCutoff.EndDate.Date && !(model.Confirmed ?? false))
            {
                if (evidencePath != null) _evidenceStorage.Delete(evidencePath);
                string endDateStr = activeCutoff.EndDate.ToString("dd/MM/yyyy");
                return Ok(new
                {
                    success = false,
                    showConfirmation = true,
                    title = "Excede el periodo",
                    message = $"El periodo actual termina el: **{endDateStr}**. ¿Desea crear el permiso aun si excede esta fecha?\n\n*Tenga en cuenta que solo tiene posibilidad entre la fecha de inicio y cierre del periodo, no más ni menos.*",
                    confirmButtonText = "Sí, crear",
                    icon = "warning"
                });
            }

            // Calcular días (lógica simple)
            int? days = null;
            if (model.StartDate != default && model.EndDate.HasValue)
            {
                days = (int)((model.EndDate.Value - model.StartDate).TotalDays + 1);
            }

            try
            {
                var permission = new Permission
                {
                    Reason = model.Reason,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    PermissionTypeId = model.PermissionTypeId,
                    RequestingUserId = requesterId,
                    WithPay = model.WithPay,
                    Evidence = evidencePath,
                    Days = days,
                    Shift = model.Shift,
                    HoursInClassroom = model.HoursInClassroom,
                    HoursDischarge = model.HoursDischarge
                };

                // El servicio aplica las reglas de negocio: valida corte activo, asigna CutoffId y
                // re-evalúa "auto-aprobado SIN goce" cuando el periodo excede el corte.
                int newId = await _permissionService.CreatePermissionAsync(permission, userId);

                return CreatedAtAction(nameof(GetPermission), new { id = newId }, new { Id = newId });
            }
            catch (InvalidOperationException ex)
            {
                if (evidencePath != null) _evidenceStorage.Delete(evidencePath);
                return BadRequest(new { success = false, title = "Validación", message = ex.Message });
            }
            catch
            {
                if (evidencePath != null) _evidenceStorage.Delete(evidencePath);
                throw;
            }
        }

        // PUT: api/Permissions/5
        [HttpPut("{id}")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> PutPermission(int id, [FromForm] PermissionApiUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest("El ID de la URL no coincide con el del cuerpo.");

            var permission = await _context.Permissions
                .Include(p => p.Cutoff)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var actorUserType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            // Regla de negocio: los jefes de área no editan solicitudes (solo las revisan/autorizan)
            if (actorUserType == (int)UserTypeEnum.Authorizer)
                return StatusCode(403, new { error = "Forbidden", message = "Los jefes de área deben usar 'Revisar' para autorizar permisos, no editar solicitudes." });

            // Regla de negocio: solo quien tiene acceso puede modificar el permiso
            if (!_permissionService.CanUserAccessPermiso(userId, actorUserType, permission))
                return StatusCode(403, new { error = "Forbidden", message = "No tiene permisos para modificar esta solicitud." });

            if (permission.PermissionStatusTypeId != (int)PermissionStatusEnum.Pending) return BadRequest("No se puede modificar un permiso ya revisado o invalidado.");

            var cutoff = permission.Cutoff;
            if (cutoff == null || cutoff.CutoffStatusTypeId != (int)CutoffStatusEnum.Open)
                return BadRequest("No se puede editar un permiso si el periodo ya está cerrado o invalidado.");

            // Encontrar Cutoff activo para la validación de exceder el periodo
            var activeCutoff = await _cutoffService.GetActiveCutoffAsync();

            if (activeCutoff != null)
            {
                if (model.StartDate.Date < activeCutoff.StartDate.Date)
                    return BadRequest(new { success = false, title = "Fuera del corte", message = "La fecha de inicio está fuera del corte activo." });

                if (model.EndDate.HasValue && model.EndDate.Value.Date > activeCutoff.EndDate.Date && !(model.Confirmed ?? false))
                {
                    string endDateStr = activeCutoff.EndDate.ToString("dd/MM/yyyy");
                    return Ok(new
                    {
                        success = false,
                        showConfirmation = true,
                        title = "Excede el periodo",
                        message = $"El periodo actual termina el: **{endDateStr}**. ¿Desea modificar el permiso aun si excede esta fecha?\n\n*Tenga en cuenta que solo tiene posibilidad entre la fecha de inicio y cierre del periodo, no más ni menos.*",
                        confirmButtonText = "Sí, guardar",
                        icon = "warning"
                    });
                }
            }

            // Evidencia nueva: validar y guardar, luego borrar la anterior si cambió
            string? oldPath = permission.Evidence;
            string? newPath = oldPath;
            if (model.EvidenceFile != null && model.EvidenceFile.Length > 0)
            {
                var evidenceError = ValidateEvidenceFile(model.EvidenceFile);
                if (evidenceError != null)
                    return BadRequest(new { success = false, title = "Validación", message = evidenceError });

                newPath = await _evidenceStorage.SaveAsync(model.EvidenceFile, permission.RequestingUserId);
            }
            else if (model.ClearEvidence == true)
            {
                newPath = null;
            }

            // Actualizar campos editables
            permission.Reason = model.Reason;
            permission.StartDate = model.StartDate;
            permission.EndDate = model.EndDate;
            permission.PermissionTypeId = model.PermissionTypeId;
            permission.WithPay = model.WithPay;
            permission.Evidence = newPath;
            permission.Shift = model.Shift;
            permission.HoursInClassroom = model.HoursInClassroom;
            permission.HoursDischarge = model.HoursDischarge;

            try
            {
                // El servicio preserva el CutoffId original y re-evalúa la regla de negocio
                // (auto-aprobado SIN goce si queda fuera del corte, o reset a Pending si vuelve a estar dentro).
                bool success = await _permissionService.UpdatePermissionAsync(permission, userId);
                if (!success)
                {
                    if (newPath != oldPath) _evidenceStorage.Delete(newPath!);
                    return BadRequest("No se pudo modificar el permiso. Verifique que siga en estado Pendiente y que el periodo esté abierto.");
                }

                // Solo si el guardado fue exitoso y subimos uno nuevo, borramos el viejo
                if (newPath != oldPath && oldPath != null) _evidenceStorage.Delete(oldPath);

                return NoContent();
            }
            catch
            {
                if (newPath != oldPath) _evidenceStorage.Delete(newPath!);
                throw;
            }
        }

        // POST: api/Permissions/5/authorize
        [HttpPost("{id}/authorize")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Actualizar")]
        public async Task<IActionResult> AuthorizePermission(int id, [FromBody] AuthorizePermissionDto model)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound(new { message = "Permiso no encontrado." });

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var actorUserType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            // Regla de negocio: solo Admin o Jefe de área pueden autorizar (RH captura, audita y revisa el flujo, pero NO autoriza)
            if (actorUserType != (int)UserTypeEnum.Administrator && actorUserType != (int)UserTypeEnum.Authorizer)
                return StatusCode(403, new { error = "Forbidden", message = "Solo los administradores o jefes de área pueden autorizar permisos." });

            // Regla de negocio: solo permisos Pendiente por Revisar pueden autorizarse/rechazarse
            if (permission.PermissionStatusTypeId != (int)PermissionStatusEnum.Pending)
                return BadRequest(new { message = "Solo se pueden autorizar o rechazar permisos en estado Pendiente por Revisar." });

            // Regla de negocio: acceso (un jefe de área solo autoriza permisos de su área)
            if (!_permissionService.CanUserAccessPermiso(userId, actorUserType, permission))
                return StatusCode(403, new { error = "Forbidden", message = "No tiene acceso para autorizar este permiso." });

            // El servicio aplica la regla WithPay = aprobado && withPay (sin goce si se rechaza)
            bool success = await _permissionService.AuthorizePermissionAsync(id, userId, model.Approved, model.WithPay, model.AuthorizationComment);
            if (!success) return NotFound(new { message = "Permiso no encontrado." });

            return Ok(new
            {
                success = true,
                message = model.Approved ? "Permiso autorizado exitosamente." : "Permiso rechazado exitosamente."
            });
        }

        // Valida que el archivo de evidencia sea PDF y no exceda el tamaño configurado
        private string? ValidateEvidenceFile(IFormFile file)
        {
            if (file.Length <= 0) return "El archivo adjunto está vacío.";

            int maxMb = _configuration.GetValue<int>("BusinessRules:Uploads:MaxFileSizeMB");
            if (file.Length > maxMb * 1024 * 1024)
                return $"El archivo excede el tamaño máximo permitido de {maxMb} MB.";

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                return "Solo se permiten archivos PDF.";

            return null;
        }

        // PUT: api/Permissions/5/cancel — el solicitante cancela su permiso pendiente
        [HttpPut("{id}/cancel")]
        [ApiKeyAuthorize(TargetTable = "Permission", RequiredPermission = "Eliminar")]
        public async Task<IActionResult> CancelPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound();

            var userId = HttpContext.Items["ApiUserId"] as int? ?? 0;
            var actorUserType = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.UserTypeId)
                .FirstOrDefaultAsync();

            // Regla de negocio: solo el solicitante (o RH/Administrador) puede cancelar.
            bool isOwner = permission.RequestingUserId == userId;
            bool esRhOrAdmin = actorUserType == (int)UserTypeEnum.HumanResources || actorUserType == (int)UserTypeEnum.Administrator;
            if (!isOwner && !esRhOrAdmin)
                return StatusCode(403, new { error = "Forbidden", message = "No tiene acceso para cancelar este permiso." });

            if (permission.PermissionStatusTypeId == (int)PermissionStatusEnum.Cancelled)
                return BadRequest(new { message = "El permiso ya está cancelado." });

            bool success = await _permissionService.CancelPermissionAsync(id, userId);
            if (!success)
                return BadRequest(new { message = "No se pudo cancelar el permiso. Solo es posible cancelarlo estando pendiente dentro de un periodo vigente." });

            return NoContent();
        }

        public class AuthorizePermissionDto
        {
            public bool Approved { get; set; } = true;
            public bool WithPay { get; set; } = true;
            public string? AuthorizationComment { get; set; }
        }
    }
}
