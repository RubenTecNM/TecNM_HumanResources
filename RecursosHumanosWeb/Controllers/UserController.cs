using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels;
using RecursosHumanosWeb.Models.ViewModels.User;
using RecursosHumanosWeb.Models.Enums;
using RecursosHumanosWeb.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;

namespace RecursosHumanosWeb.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public UserController(
            IMapper mapper,
            ILogger<UserController> logger,
            IUsuarioService usuarioService,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _logger = logger;
            _usuarioService = usuarioService;
            _configuration = configuration;
        }

        #region INDEX Y BÚSQUEDA

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Index(
            string nombreFilter,
            string correoFilter,
            int? departamentoFilter,
            bool? estadoFilter,
            int? page,
            string sortOrder,
            string currentFilter)
        {
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");
            
            if (!_usuarioService.CanUserManageUsuarios(userType))
                return Forbid();

            ViewData["IsAdministrador"] = userType == (int)UserTypeEnum.Administrator;

            // Parámetros de ordenamiento
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NombreSortParm = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
            ViewBag.CorreoSortParm = sortOrder == "correo" ? "correo_desc" : "correo";
            ViewBag.DepartamentoSortParm = sortOrder == "department" ? "departamento_desc" : "department";
            ViewBag.FechaSortParm = sortOrder == "fecha" ? "fecha_desc" : "fecha";

            // Lógica de filtrado y paginación corregida
            if (nombreFilter != null || correoFilter != null || departamentoFilter != null || estadoFilter != null)
            {
                // Si es una nueva búsqueda (sin parámetro de página), empezamos en 1
                if (!page.HasValue) page = 1;
            }
            else
            {
                // Si no hay parámetros nuevos, recuperamos el filtro actual
                nombreFilter = currentFilter;
            }

            ViewBag.CurrentFilter = nombreFilter;
            
            int pageSize = _configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize");
            int pageNumber = page ?? 1;

            var paginatedUsers = await _usuarioService.GetUsuariosPagedAsync(
                nombreFilter,
                correoFilter,
                departamentoFilter,
                estadoFilter,
                pageNumber,
                pageSize,
                sortOrder ?? "");

            var areas = await _usuarioService.GetAreasAsync();
            var departments = await _usuarioService.GetDepartamentosAsync();

            var model = new UserSearchViewModel
            {
                UsuariosPaginados = paginatedUsers,
                Areas = areas,
                Departments = departments
            };

            ViewBag.NombreFilter = nombreFilter;
            ViewBag.CorreoFilter = correoFilter;
            ViewBag.DepartamentoFilter = departamentoFilter;
            ViewBag.EstadoFilter = estadoFilter;

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var results = await _usuarioService.SearchUsuariosAsync(term ?? "");
            // Mapear a un formato que Select2 entienda por defecto (id, text) y agregar metadatos necesarios
            var select2Results = results.Select(u => new
            {
                id = u.Id,
                text = u.Name,
                areaId = u.AreaId
            });
            return Json(select2Results);
        }

        #endregion

        #region DETAILS

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            bool isRHOrAdmin = User.HasClaim(c => c.Type == "UserType" && (c.Value == "1" || c.Value == "3"));

            // Si no es RH/Admin y el ID no coincide con su propia cuenta, denegar
            if (!isRHOrAdmin && id != currentUserId)
            {
                return Forbid();
            }

            var vm = await _usuarioService.GetUsuarioDetailsWithStatsAsync(id.Value);
            if (vm == null) return NotFound();

            return View(vm);
        }

        #endregion

        #region CREATE

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Create()
        {
            await PrepareUserSelectListsAsync();
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareUserSelectListsAsync();
                return View(model);
            }

            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            try
            {
                var user = _mapper.Map<User>(model);
                await _usuarioService.CreateUsuarioAsync(user, currentUserId, model.Password);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear user. Detalle: {Inner}", inner);
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}. Detalle: {inner}");
            }

            await PrepareUserSelectListsAsync();
            return View(model);
        }

        #endregion

        #region EDIT

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _usuarioService.GetUsuarioByIdAsync(id.Value);
            if (user == null) return NotFound();

            var vm = _mapper.Map<UserUpdateViewModel>(user);
            vm.Password = null; // No prellenar contraseña

            await PrepareUserSelectListsAsync(user.CreatedByUserId, user.UpdatedByUserId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Edit(int id, UserUpdateViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await PrepareUserSelectListsAsync();
                return View(model);
            }

            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            try
            {
                var user = _mapper.Map<User>(model);
                var success = await _usuarioService.UpdateUsuarioAsync(user, currentUserId, model.Password);

                if (!success)
                {
                    ModelState.AddModelError("", "No se pudo actualizar el usuario.");
                    await PrepareUserSelectListsAsync();
                    return View(model);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al actualizar user {Id}. Detalle: {Inner}", id, inner);
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}. Detalle: {inner}");
            }

            await PrepareUserSelectListsAsync();
            return View(model);
        }

        #endregion

        #region DELETE

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _usuarioService.GetUsuarioByIdAsync(id.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            var success = await _usuarioService.DeleteUsuarioAsync(id, currentUserId);
            
            if (!success)
            {
                TempData["ErrorAlert"] = System.Text.Json.JsonSerializer.Serialize(
                    new { icon = "error", title = "Error", text = "No se puede eliminar al usuario (quizá es tu propia cuenta)." });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> DeleteAjax([FromBody] ActionRequestDTO request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error",
                    Message = "ID no válido.",
                    Icon = "error"
                });
            }

            var user = await _usuarioService.GetUsuarioByIdAsync(request.Id);

            if (user == null || !user.Status)
            {
                return Json(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error",
                    Message = "El usuario no fue encontrado o ya está inactivo.",
                    Icon = "error"
                });
            }

            if (!request.Confirmed)
            {
                return Json(new AlertResponseDTO
                {
                    ShowConfirmation = true,
                    Title = "¿Eliminar User?",
                    Message = $"¿Estás seguro de que deseas eliminar al usuario **{user.Name}**?",
                    Icon = "question",
                    ConfirmButtonText = "Sí, Eliminar"
                });
            }

            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            try
            {
                var success = await _usuarioService.DeleteUsuarioAsync(request.Id, currentUserId);

                if (!success)
                {
                    return Json(new AlertResponseDTO
                    {
                        Success = false,
                        Title = "Error",
                        Message = "No puedes eliminar tu propia cuenta.",
                        Icon = "error"
                    });
                }

                return Json(new AlertResponseDTO
                {
                    Success = true,
                    Title = "¡Eliminado!",
                    Message = $"El user **{user.Name}** ha sido eliminado.",
                    Icon = "success",
                    RedirectUrl = "/User/Index"
                });
            }
            catch (Exception ex)
            {
                return Json(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error del Servidor",
                    Message = $"Ocurrió un error: {ex.Message}",
                    Icon = "error"
                });
            }
        }

        #endregion

        #region HELPERS

        private async Task PrepareUserSelectListsAsync(int? creaId = null, int? modId = null)
        {
            var areas = await _usuarioService.GetAreasAsync();
            var departments = await _usuarioService.GetDepartamentosAsync();
            var positions = await _usuarioService.GetPuestosAsync();
            var userTypes = await _usuarioService.GetTipoUsuariosAsync();

            ViewData["AreaId"] = new SelectList(areas, "Id", "Description");
            ViewData["DepartmentId"] = new SelectList(departments, "Id", "Description");
            ViewData["PositionId"] = new SelectList(positions, "Id", "Description");
            ViewData["UserTypeId"] = new SelectList(userTypes, "Id", "Description");
            
            if (creaId.HasValue || modId.HasValue)
            {
                var users = await _usuarioService.GetActiveUsuariosAsync();
                ViewData["CreatedByUserId"] = new SelectList(users, "Id", "Name", creaId);
                ViewData["UpdatedByUserId"] = new SelectList(users, "Id", "Name", modId);
            }
        }

        #endregion
    }
}