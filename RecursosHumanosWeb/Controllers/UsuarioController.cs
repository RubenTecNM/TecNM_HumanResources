using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecursosHumanosWeb.Models;
using RecursosHumanosWeb.Models.DTOs;
using RecursosHumanosWeb.Models.ViewModels;
using RecursosHumanosWeb.Models.ViewModels.Usuario;
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
    public class UsuarioController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILogger<UsuarioController> _logger;
        private readonly IUsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public UsuarioController(
            IMapper mapper,
            ILogger<UsuarioController> logger,
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
            int userType = int.Parse(User.FindFirst("TipoUsuario")?.Value ?? "0");
            
            if (!_usuarioService.CanUserManageUsuarios(userType))
                return Forbid();

            ViewData["IsAdministrador"] = userType == (int)TipoUsuarioEnum.Administrador;

            // Parámetros de ordenamiento
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NombreSortParm = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
            ViewBag.CorreoSortParm = sortOrder == "correo" ? "correo_desc" : "correo";
            ViewBag.DepartamentoSortParm = sortOrder == "departamento" ? "departamento_desc" : "departamento";
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

            var usuariosPaginados = await _usuarioService.GetUsuariosPagedAsync(
                nombreFilter,
                correoFilter,
                departamentoFilter,
                estadoFilter,
                pageNumber,
                pageSize,
                sortOrder ?? "");

            var areas = await _usuarioService.GetAreasAsync();
            var departamentos = await _usuarioService.GetDepartamentosAsync();

            var model = new UsuarioSearchViewModel
            {
                UsuariosPaginados = usuariosPaginados,
                Areas = areas,
                Departamentos = departamentos
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
                text = u.Nombre,
                areaId = u.IdArea
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
            bool isRHOrAdmin = User.HasClaim(c => c.Type == "TipoUsuario" && (c.Value == "1" || c.Value == "3"));

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
            return View(new UsuarioCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Create(UsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareUserSelectListsAsync();
                return View(model);
            }

            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            try
            {
                var usuario = _mapper.Map<Usuario>(model);
                await _usuarioService.CreateUsuarioAsync(usuario, currentUserId, model.Clave);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "No inner exception";
                _logger.LogError(ex, "Error al crear usuario. Detalle: {Inner}", inner);
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

            var usuario = await _usuarioService.GetUsuarioByIdAsync(id.Value);
            if (usuario == null) return NotFound();

            var vm = _mapper.Map<UsuarioUpdateViewModel>(usuario);
            vm.Clave = null; // No prellenar contraseña

            await PrepareUserSelectListsAsync(usuario.IdUsuarioCrea, usuario.IdUsuarioModifica);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RHOrAdmin")]
        public async Task<IActionResult> Edit(int id, UsuarioUpdateViewModel model)
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
                var usuario = _mapper.Map<Usuario>(model);
                var success = await _usuarioService.UpdateUsuarioAsync(usuario, currentUserId, model.Clave);

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
                _logger.LogError(ex, "Error al actualizar usuario {Id}. Detalle: {Inner}", id, inner);
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

            var usuario = await _usuarioService.GetUsuarioByIdAsync(id.Value);
            if (usuario == null) return NotFound();

            return View(usuario);
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
                    new { icon = "error", title = "Error", text = "No se puede eliminar el usuario (quizá es tu propia cuenta)." });
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

            var usuario = await _usuarioService.GetUsuarioByIdAsync(request.Id);

            if (usuario == null || !usuario.Estatus)
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
                    Title = "¿Eliminar Usuario?",
                    Message = $"¿Estás seguro de que deseas eliminar al usuario **{usuario.Nombre}**?",
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
                    Message = $"El usuario **{usuario.Nombre}** ha sido eliminado.",
                    Icon = "success",
                    RedirectUrl = "/Usuario/Index"
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
            var departamentos = await _usuarioService.GetDepartamentosAsync();
            var puestos = await _usuarioService.GetPuestosAsync();
            var tipoUsuarios = await _usuarioService.GetTipoUsuariosAsync();

            ViewData["IdArea"] = new SelectList(areas, "Id", "Descripcion");
            ViewData["IdDepartamento"] = new SelectList(departamentos, "Id", "Descripcion");
            ViewData["IdPuesto"] = new SelectList(puestos, "Id", "Descripcion");
            ViewData["IdTipoUsuario"] = new SelectList(tipoUsuarios, "Id", "Descripcion");
            
            if (creaId.HasValue || modId.HasValue)
            {
                var usuarios = await _usuarioService.GetActiveUsuariosAsync();
                ViewData["IdUsuarioCrea"] = new SelectList(usuarios, "Id", "Nombre", creaId);
                ViewData["IdUsuarioModifica"] = new SelectList(usuarios, "Id", "Nombre", modId);
            }
        }

        #endregion
    }
}