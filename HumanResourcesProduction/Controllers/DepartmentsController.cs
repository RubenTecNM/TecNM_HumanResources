using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.DTOs;
using HumanResourcesProduction.Models.ViewModels.Departments;
using X.PagedList.Extensions;

namespace HumanResourcesProduction.Controllers;

[Authorize(Policy = "RHOrAdmin")]
public class DepartmentsController : Controller
{
    private readonly HumanResourcesContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        HumanResourcesContext context,
        IConfiguration configuration,
        ILogger<DepartmentsController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? descriptionFilter, bool? statusFilter, int page = 1)
    {
        var query = _context.Departments.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(descriptionFilter))
        {
            var filter = descriptionFilter.Trim();
            query = query.Where(d => d.Description.Contains(filter));
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(d => d.Status == statusFilter.Value);
        }

        var pageSize = _configuration.GetValue<int>("BusinessRules:Pagination:DefaultPageSize");
        var departments = await query
            .OrderBy(d => d.Description)
            .ToListAsync();

        ViewBag.DescriptionFilter = descriptionFilter;
        ViewBag.StatusFilter = statusFilter;
        return View(departments.ToPagedList(Math.Max(page, 1), pageSize));
    }

    [HttpGet]
    public IActionResult Create() => View(new DepartmentCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var description = model.Description.Trim();
        if (await _context.Departments.AnyAsync(d => d.Description == description))
        {
            ModelState.AddModelError(nameof(model.Description), "Ya existe un departamento con esta descripción.");
            return View(model);
        }

        var currentUserId = GetCurrentUserId();
        var department = new Department
        {
            Description = description,
            Status = true,
            CreatedAt = DateTime.Now,
            CreatedByUserId = currentUserId
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "El departamento se creó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (department is null) return NotFound();

        return View(new DepartmentUpdateViewModel { Id = department.Id, Description = department.Description });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentUpdateViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (department is null) return NotFound();

        var description = model.Description.Trim();
        if (await _context.Departments.AnyAsync(d => d.Id != id && d.Description == description))
        {
            ModelState.AddModelError(nameof(model.Description), "Ya existe un departamento con esta descripción.");
            return View(model);
        }

        department.Description = description;
        department.UpdatedAt = DateTime.Now;
        department.UpdatedByUserId = GetCurrentUserId();
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "El departamento se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax([FromBody] ActionRequestDTO request)
    {
        if (request is null || request.Id <= 0)
        {
            return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "ID de departamento no válido.", Icon = "error" });
        }

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.Id);
        if (department is null || !department.Status)
        {
            return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "El departamento no fue encontrado o ya está eliminado.", Icon = "error" });
        }

        if (!request.Confirmed)
        {
            return Json(new AlertResponseDTO
            {
                ShowConfirmation = true,
                Title = "¿Eliminar departamento?",
                Message = $"¿Deseas eliminar **{department.Description}**? Esta acción es un borrado lógico: el registro se conserva, pero dejará de estar disponible en la aplicación.",
                Icon = "warning",
                ConfirmButtonText = "Sí, eliminar"
            });
        }

        try
        {
            department.Status = false;
            department.UpdatedAt = DateTime.Now;
            department.UpdatedByUserId = GetCurrentUserId();
            await _context.SaveChangesAsync();
            return Json(new AlertResponseDTO("Departments", "El departamento fue eliminado lógicamente."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unable to logically delete department {DepartmentId}.", request.Id);
            return Json(new AlertResponseDTO { Success = false, Title = "Error", Message = "No fue posible eliminar el departamento.", Icon = "error" });
        }
    }

    private int GetCurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 1;
}
