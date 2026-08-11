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
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(
            IMapper mapper,
            ILogger<UserController> logger,
            IUserService userService,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
        }

        #region INDEX Y BÚSQUEDA

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Index(
            string nameFilter,
            string emailFilter,
            int? departmentFilter,
            bool? statusFilter,
            int? page,
            string sortOrder,
            string currentFilter)
        {
            int userType = int.Parse(User.FindFirst("UserType")?.Value ?? "0");
            
            if (!_userService.CanUserManageUsers(userType))
                return Forbid();

            ViewData["IsAdministrator"] = userType == (int)UserTypeEnum.Administrator;

            // Parámetros de ordenamiento
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.EmailSortParm = sortOrder == "email" ? "email_desc" : "email";
            ViewBag.DepartmentSortParm = sortOrder == "department" ? "department_desc" : "department";
            ViewBag.DateSortParm = sortOrder == "date" ? "date_desc" : "date";

            // Lógica de filtrado y paginación corregida
            if (nameFilter != null || emailFilter != null || departmentFilter != null || statusFilter != null)
            {
                // Si es una nueva búsqueda (sin parámetro de página), empezamos en 1
                if (!page.HasValue) page = 1;
            }
            else
            {
                // Si no hay parámetros nuevos, recuperamos el filtro actual
                nameFilter = currentFilter;
            }

            ViewBag.CurrentFilter = nameFilter;
            
            int pageSize = _configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize");
            int pageNumber = page ?? 1;

            var paginatedUsers = await _userService.GetUsersPagedAsync(
                nameFilter,
                emailFilter,
                departmentFilter,
                statusFilter,
                pageNumber,
                pageSize,
                sortOrder ?? "");

            var areas = await _userService.GetAreasAsync();
            var departments = await _userService.GetDepartmentsAsync();

            var model = new UserSearchViewModel
            {
                PagedUsers = paginatedUsers,
                Areas = areas,
                Departments = departments
            };

            ViewBag.NameFilter = nameFilter;
            ViewBag.EmailFilter = emailFilter;
            ViewBag.DepartmentFilter = departmentFilter;
            ViewBag.StatusFilter = statusFilter;

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var results = await _userService.SearchUsersAsync(term ?? "");
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

            var vm = await _userService.GetUserDetailsWithStatsAsync(id.Value);
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
                await _userService.CreateUserAsync(user, currentUserId, model.Password);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear user. Detalle: {Inner}", inner);
                ModelState.AddModelError("", "Error al guardar el usuario. Inténtelo nuevamente.");
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

            var user = await _userService.GetUserByIdAsync(id.Value);
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
                var success = await _userService.UpdateUserAsync(user, currentUserId, model.Password);

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
                ModelState.AddModelError("", "Error al actualizar el usuario. Inténtelo nuevamente.");
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

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            var success = await _userService.DeleteUserAsync(id, currentUserId);
            
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

            var user = await _userService.GetUserByIdAsync(request.Id);

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
                var success = await _userService.DeleteUserAsync(request.Id, currentUserId);

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
                _logger.LogError(ex, "Error al eliminar user {Id}.", request.Id);
                return Json(new AlertResponseDTO
                {
                    Success = false,
                    Title = "Error del Servidor",
                    Message = "Ocurrió un error inesperado al eliminar el usuario. Inténtelo nuevamente.",
                    Icon = "error"
                });
            }
        }

        #endregion

        #region HELPERS

        private async Task PrepareUserSelectListsAsync(int? creaId = null, int? modId = null)
        {
            var areas = await _userService.GetAreasAsync();
            var departments = await _userService.GetDepartmentsAsync();
            var positions = await _userService.GetPositionsAsync();
            var userTypes = await _userService.GetUserTypesAsync();

            ViewData["AreaId"] = new SelectList(areas, "Id", "Description");
            ViewData["DepartmentId"] = new SelectList(departments, "Id", "Description");
            ViewData["PositionId"] = new SelectList(positions, "Id", "Description");
            ViewData["UserTypeId"] = new SelectList(userTypes, "Id", "Description");
            
            if (creaId.HasValue || modId.HasValue)
            {
                var users = await _userService.GetActiveUsersAsync();
                ViewData["CreatedByUserId"] = new SelectList(users, "Id", "Name", creaId);
                ViewData["UpdatedByUserId"] = new SelectList(users, "Id", "Name", modId);
            }
        }

        #endregion
    }
}