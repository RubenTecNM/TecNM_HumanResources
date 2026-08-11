using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RecursosHumanosWeb.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class ApiKeyController : Controller
    {
        #region FIELDS & CONSTRUCTOR
        private readonly HumanResourcesContext _context;
        private readonly AutoMapper.IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyController> _logger;

        public ApiKeyController(HumanResourcesContext context, AutoMapper.IMapper mapper, IConfiguration configuration, ILogger<ApiKeyController> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }
        #endregion

        #region INDEX
        // GET: ApiKeys
        public async Task<IActionResult> Index(string userName, DateTime? fromDay, DateTime? toDay)
        {
            var query = _context.ApiKeys
                .Include(a => a.CreatedByUser)
                .Include(a => a.UpdatedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(a => a.CreatedByUser.Name.Contains(userName));
            }

            if (fromDay.HasValue)
            {
                query = query.Where(a => a.CreatedAt.Date >= fromDay.Value.Date);
            }

            if (toDay.HasValue)
            {
                query = query.Where(a => a.CreatedAt.Date <= toDay.Value.Date);
            }

            ViewBag.UserName = userName;
            ViewBag.FromDay = fromDay?.ToString("yyyy-MM-dd");
            ViewBag.Today = toDay?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(a => a.CreatedAt).ToListAsync());
        }
        #endregion

        #region DETAILS
        // GET: ApiKeys/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys
                .Include(a => a.CreatedByUser)
                .Include(a => a.UpdatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (apiKey == null)
            {
                return NotFound();
            }

            // Cargar permissions asignados agrupados por tabla
            var assignedPermissions = await _context.ApiKeyTablePermissions
                .Where(p => p.ApiKeyId == id && p.Status)
                .Include(p => p.Table)
                .Include(p => p.ApiPermission)
                .GroupBy(p => p.Table.Description)
                .Select(g => new
                {
                    DatabaseTable = g.Key,
                    Permissions = g.Select(p => p.ApiPermission.Description).ToList()
                })
                .ToListAsync();

            ViewBag.AssignedPermissions = assignedPermissions;

            return View(apiKey);
        }
        #endregion

        #region CREATE
        // GET: ApiKeys/Create
        public IActionResult Create()
        {
            // CORRECCIÓN: Usamos proyección segura para evitar nulos en el SelectList
            var activeUsers = _context.Users
                .Where(u => u.Status)
                .Select(u => new { u.Id, u.Name }) // Solo traemos lo necesario
                .OrderBy(u => u.Name)
                .ToList();

            // Usamos "Name" que es la propiedad anónima definida arriba
            ViewData["User"] = new SelectList(activeUsers, "Id", "Name");

            ViewBag.DatabaseTables = _context.DatabaseTables.Where(t => t.Status).ToList();
            ViewBag.ApiPermissions = _context.ApiPermissions.Where(p => p.Status).ToList();

            // Lógica de roles (ajusta según tus necesidades reales)
            ViewBag.IsAdministrator = User.HasClaim("UserType", "1");

            return View(new ApiKeyCreateViewModel());
        }

        // POST: ApiKeys/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApiKeyCreateViewModel viewModel)
        {
            // 1. OBTENER ID REAL DEL USUARIO LOGUEADO
            // Parseamos el Claim. Si falla o es nulo, asigna 0.
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                ModelState.AddModelError("", "No se pudo identificar al usuario en sesión.");
            }

            // 2. LÓGICA DE TITULARIDAD
            // Si el dropdown vino vacío (null), asignamos el user logueado.
            // Si el dropdown trajo un ID, respetamos ese ID (viewModel.OwnerUserId).
            if (!viewModel.OwnerUserId.HasValue)
            {
                viewModel.OwnerUserId = userId;
            }

            // VALIDACIÓN DEL MODELO
            if (!ModelState.IsValid || viewModel.OwnerUserId == null || userId == 0)
            {
                // Recargar listas en caso de error (con la misma lógica segura del GET)
                var usersList = _context.Users
                                        .Where(u => u.Status)
                                        .Select(u => new { u.Id, u.Name })
                                        .OrderBy(u => u.Name)
                                        .ToList();

                ViewData["User"] = new SelectList(usersList, "Id", "Name", viewModel.OwnerUserId);
                ViewBag.DatabaseTables = _context.DatabaseTables.Where(t => t.Status).ToList();
                ViewBag.ApiPermissions = _context.ApiPermissions.Where(p => p.Status).ToList();
                ViewBag.IsAdministrator = User.HasClaim("UserType", "1");

                return View(viewModel);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentDateTime = DateTime.Now; // Fecha uniforme para todo el proceso

                    // 3. CREAR API KEY
                    var newApiKey = new ApiKey
                    {
                        Key = Guid.NewGuid().ToString("N"),
                        Status = true,
                        CreatedAt = currentDateTime,

                        // CORRECCIÓN CRÍTICA: DateTime.Now en lugar de default
                        UpdatedAt = currentDateTime,

                        CreatedByUserId = userId,
                        UpdatedByUserId = userId

                        // Nota: Si tienes un campo OwnerUserId en la tabla ApiKey, asígnalo aquí:
                        // OwnerUserId = viewModel.OwnerUserId.Value 
                    };

                    _context.Add(newApiKey);
                    await _context.SaveChangesAsync(); // Se genera el ID de la ApiKey

                    // 4. CREAR PERMISOS (ApiKeyTablePermission)
                    if (viewModel.Assignments != null)
                    {
                        foreach (var assignment in viewModel.Assignments)
                        {
                            if (assignment.IdsApiPermissionsSeleccionados != null)
                            {
                                foreach (var permissionId in assignment.IdsApiPermissionsSeleccionados.Distinct())
                                {
                                    var newAssignment = new ApiKeyTablePermission
                                    {
                                        ApiKeyId = newApiKey.Id, // Usamos el ID recién creado
                                        ApiPermissionId = permissionId,
                                        TableId = assignment.TableId,
                                        Status = true,
                                        CreatedAt = currentDateTime,

                                        // CORRECCIÓN CRÍTICA: DateTime.Now en lugar de default
                                        UpdatedAt = currentDateTime,

                                        CreatedByUserId = userId,
                                        UpdatedByUserId = userId,
                                    };
                                    _context.Add(newAssignment);
                                }
                            }
                        }
                        // Guardamos los permissions dentro de la transacción
                        await _context.SaveChangesAsync();
                    }

                    // 5. CONFIRMAR TRANSACCIÓN
                    // Al no haber errores de fecha, esto se ejecutará y guardará ambas tablas.
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"API Key generada con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al generar la API Key.");
                    ModelState.AddModelError("", "Error al generar la API Key. Inténtelo nuevamente.");

                    // Recargar datos para la vista (Copy-paste de la lógica de arriba)
                    var usersList = _context.Users
                                            .Where(u => u.Status)
                                            .Select(u => new { u.Id, u.Name })
                                            .OrderBy(u => u.Name)
                                            .ToList();
                    ViewData["User"] = new SelectList(usersList, "Id", "Name", viewModel.OwnerUserId);
                    ViewBag.DatabaseTables = _context.DatabaseTables.Where(t => t.Status).ToList();
                    ViewBag.ApiPermissions = _context.ApiPermissions.Where(p => p.Status).ToList();
                    ViewBag.IsAdministrator = User.HasClaim("UserType", "1");

                    return View(viewModel);
                }
            }
        }
        #endregion

        #region EDIT
        // GET: ApiKeys/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null)
            {
                return NotFound();
            }

            // Cargar permissions actuales
            var currentPermissions = await _context.ApiKeyTablePermissions
                .Where(p => p.ApiKeyId == id && p.Status)
                .GroupBy(p => p.TableId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(p => p.ApiPermissionId).ToList()
                );

            var viewModel = new ApiKeyCreateViewModel
            {
                Id = apiKey.Id,
                Key = apiKey.Key,
                CreatedAt = apiKey.CreatedAt,
                OwnerUserId = apiKey.CreatedByUserId // Asumiendo que el titular es quien creó
            };

            // Preparar listas para la vista
            var activeUsers = _context.Users
                .Where(u => u.Status)
                .Select(u => new { u.Id, u.Name })
                .OrderBy(u => u.Name)
                .ToList();

            ViewData["User"] = new SelectList(activeUsers, "Id", "Name", viewModel.OwnerUserId);
            ViewBag.DatabaseTables = _context.DatabaseTables.Where(t => t.Status).ToList();
            ViewBag.ApiPermissions = _context.ApiPermissions.Where(p => p.Status).ToList();
            ViewBag.CurrentPermissions = currentPermissions;
            ViewBag.IsAdministrator = User.HasClaim("UserType", "1");

            return View(viewModel);
        }

        // POST: ApiKeys/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ApiKeyCreateViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                ModelState.AddModelError("", "No se pudo identificar al usuario en sesión.");
            }

            if (!ModelState.IsValid || userId == 0)
            {
                // Recargar listas en caso de error
                var usersList = _context.Users
                    .Where(u => u.Status)
                    .Select(u => new { u.Id, u.Name })
                    .OrderBy(u => u.Name)
                    .ToList();

                ViewData["User"] = new SelectList(usersList, "Id", "Name", viewModel.OwnerUserId);
                ViewBag.DatabaseTables = _context.DatabaseTables.Where(t => t.Status).ToList();
                ViewBag.ApiPermissions = _context.ApiPermissions.Where(p => p.Status).ToList();
                ViewBag.IsAdministrator = User.HasClaim("UserType", "1");

                return View(viewModel);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentDateTime = DateTime.Now;

                    // Actualizar ApiKey
                    var apiKey = await _context.ApiKeys.FindAsync(id);
                    if (apiKey == null)
                    {
                        return NotFound();
                    }

                    apiKey.UpdatedAt = currentDateTime;
                    apiKey.UpdatedByUserId = userId;
                    _context.Update(apiKey);
                    await _context.SaveChangesAsync();

                    // Eliminar permissions anteriores (soft delete)
                    var previousPermissions = await _context.ApiKeyTablePermissions
                        .Where(p => p.ApiKeyId == id)
                        .ToListAsync();

                    foreach (var permission in previousPermissions)
                    {
                        permission.Status = false;
                        permission.UpdatedAt = currentDateTime;
                        permission.UpdatedByUserId = userId;
                    }
                    await _context.SaveChangesAsync();

                    // Crear nuevos permissions
                    if (viewModel.Assignments != null)
                    {
                        foreach (var assignment in viewModel.Assignments)
                        {
                            if (assignment.IdsApiPermissionsSeleccionados != null)
                            {
                                foreach (var permissionId in assignment.IdsApiPermissionsSeleccionados.Distinct())
                                {
                                    var newAssignment = new ApiKeyTablePermission
                                    {
                                        ApiKeyId = apiKey.Id,
                                        ApiPermissionId = permissionId,
                                        TableId = assignment.TableId,
                                        Status = true,
                                        CreatedAt = currentDateTime,
                                        UpdatedAt = currentDateTime,
                                        CreatedByUserId = userId,
                                        UpdatedByUserId = userId,
                                    };
                                    _context.Add(newAssignment);
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    TempData["SuccessAlert"] = "API Key actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al actualizar la API Key.");
                    ModelState.AddModelError("", "Error al actualizar la API Key. Inténtelo nuevamente.");

                    // Recargar listas
                    var usersList = _context.Users
                        .Where(u => u.Status)
                        .Select(u => new { u.Id, u.Name })
                        .OrderBy(u => u.Name)
                        .ToList();

                    ViewData["User"] = new SelectList(usersList, "Id", "Name", viewModel.OwnerUserId);
                    ViewBag.DatabaseTables = _context.DatabaseTables.Where(t => t.Status).ToList();
                    ViewBag.ApiPermissions = _context.ApiPermissions.Where(p => p.Status).ToList();
                    ViewBag.IsAdministrator = User.HasClaim("UserType", "1");

                    return View(viewModel);
                }
            }
        }
        #endregion

        #region DELETE
        // GET: ApiKeys/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var apiKey = await _context.ApiKeys
                .Include(a => a.CreatedByUser)
                .Include(a => a.UpdatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (apiKey == null)
            {
                return NotFound();
            }

            return View(apiKey);
        }

        // POST: ApiKeys/Delete/5 (form POST) - borrado lógico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey != null)
            {
                apiKey.Status = false;
                apiKey.UpdatedAt = DateTime.Now;
                _context.Update(apiKey);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Delete via fetch with confirmation flow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax([FromBody] ActionRequestDTO request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "ID no válido.", Icon = "error" });
            }

            var apiKey = await _context.ApiKeys.FindAsync(request.Id);
            if (apiKey == null)
            {
                return Json(new AlertResponseDTO { Success = false, Title = "No encontrado", Message = "API Key no encontrada.", Icon = "error" });
            }

            if (!request.Confirmed)
            {
                // Truncar clave de forma segura
                var claveDisplay = apiKey.Key.Length > 16 
                    ? apiKey.Key.Substring(0, 16) + "..." 
                    : apiKey.Key;
                
                return Json(new AlertResponseDTO
                {
                    ShowConfirmation = true,
                    Title = "¿Eliminar API Key?",
                    Message = $"¿Estás seguro de que deseas eliminar la API Key **{claveDisplay}**? Esta acción es un borrado lógico.",
                    Icon = "question",
                    ConfirmButtonText = "Sí, eliminar"
                });
            }

            try
            {
                // Obtener user actual
                int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Borrado lógico
                apiKey.Status = false;
                apiKey.UpdatedAt = DateTime.Now;
                apiKey.UpdatedByUserId = userId;
                _context.Update(apiKey);
                await _context.SaveChangesAsync();
                
                return Json(new AlertResponseDTO
                {
                    Success = true,
                    Title = "¡Eliminado!",
                    Message = "API Key inactivada correctamente.",
                    Icon = "success",
                    RedirectUrl = "/ApiKeys/Index"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inactivar API Key.");
                return Json(new AlertResponseDTO { Success = false, Title = "Error del Servidor", Message = "Ocurrió un error al inactivar la API Key. Inténtelo nuevamente.", Icon = "error" });
            }
        }
        #endregion

        #region AJAX & HELPERS
        [HttpGet]
        public async Task<JsonResult> GetUsersForSearch(string term)
        {
            // Si no hay término de búsqueda, devolvemos una lista vacía o los primeros 10.
            if (string.IsNullOrWhiteSpace(term))
            {
                term = string.Empty;
            }

            var users = await _context.Users
                .Where(u => u.Name.Contains(term) || u.Email.Contains(term))
                .OrderBy(u => u.Name)
                .Take(_configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize")) // Limitar resultados para eficiencia
                .Select(u => new
                {
                    id = u.Id,
                    text = u.Name ?? u.Email
                })
                .ToListAsync();

            return Json(new { results = users });
        }

        private bool ApiKeyExists(int id)
        {
            return _context.ApiKeys.Any(e => e.Id == id);
        }
        #endregion
    }
}
