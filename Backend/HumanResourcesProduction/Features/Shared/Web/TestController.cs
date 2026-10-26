using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HumanResourcesProduction.Models;
using HumanResourcesProduction.Models.DTOs;
using HumanResourcesProduction.Models.Enums;

namespace HumanResourcesProduction.Features.Shared.Web
{
    /// <summary>
    /// Controlador de pruebas para validar casos de negocio durante desarrollo.
    /// Solo debe estar activo en entornos de Development.
    /// </summary>
    [ApiController]
    [Route("Test")]
    [Authorize(Policy = "AdminOnly")]
    public class TestController : ControllerBase
    {
        private readonly HumanResourcesContext _context;

        public TestController(HumanResourcesContext context)
        {
            _context = context;
        }

        // GET: /Test/RunCase?case=1
        [HttpGet("RunCase")]
        public async Task<IActionResult> RunCase(int @case = 1, bool confirmed = false)
        {
            // Buscar cutoff activo
            var activeCutoff = await _context.Cutoffs.Where(c => c.CutoffStatusTypeId == (int)CutoffStatusEnum.Open).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
            if (activeCutoff == null)
                return BadRequest(new AlertResponseDTO { Success = false, Title = "Error de Corte", Message = "No se encontró un corte activo." });

            // User RH de pruebas (si existe)
            var user = await _context.Users.Where(u => u.UserTypeId == 2 && u.Status).FirstOrDefaultAsync();
            if (user == null)
                return BadRequest(new AlertResponseDTO { Success = false, Title = "Error de Usuario", Message = "No se encontró un usuario RH de prueba." });

            if (@case == 1)
            {
                // Caso 1: StartDate anterior al inicio del cutoff
                var date1 = activeCutoff.StartDate.AddHours(-1);
                if (date1 < activeCutoff.StartDate)
                {
                    var dto = new AlertResponseDTO { Success = false, Title = "Error de Validación", Message = "La fecha y hora de inicio debe estar dentro del periodo del corte vigente." };
                    dto.Errors = new System.Collections.Generic.Dictionary<string, string[]> { { "StartDate", new[] { "La fecha y hora de inicio debe estar dentro del periodo del corte vigente." } } };
                    return Ok(dto);
                }
            }

            if (@case == 2)
            {
                // Caso 2: EndDate posterior al término del cutoff
                var date1 = activeCutoff.StartDate.AddDays(1);
                var date2 = activeCutoff.EndDate.AddDays(2);
                if (date2 > activeCutoff.EndDate && !confirmed)
                {
                    return Ok(new AlertResponseDTO
                    {
                        Success = false,
                        ShowConfirmation = true,
                        Title = "Fechas fuera del corte",
                        Message = "Las fechas proporcionadas exceden el periodo del corte vigente. ¿Desea crear el permiso de todas formas?",
                        Icon = "warning",
                        ConfirmButtonText = "Sí, crear permiso"
                    });
                }

                if (confirmed)
                {
                    var permission = new Permission
                    {
                        Reason = "Test confirmado fuera de cutoff",
                        StartDate = activeCutoff.StartDate.AddDays(1),
                        EndDate = activeCutoff.EndDate.AddDays(2),
                        Days = (int)((activeCutoff.EndDate.AddDays(2) - activeCutoff.StartDate.AddDays(1)).TotalDays + 1),
                        CutoffId = activeCutoff.Id,
                        PermissionTypeId = 2,
                        RequestingUserId = user.Id,
                        CreatedByUserId = user.Id,
                        CreatedAt = DateTime.Now,
                        PermissionStatusTypeId = (int)PermissionStatusEnum.Pending,
                        WithPay = false
                    };
                    _context.Permissions.Add(permission);
                    await _context.SaveChangesAsync();
                    return Ok(new AlertResponseDTO("Permission", $"Permission creado (ID: {permission.Id})"));
                }
            }

            if (@case == 3)
            {
                // Caso 3: Fechas dentro del cutoff
                var date1 = activeCutoff.StartDate.AddDays(1).Date;
                var date2 = activeCutoff.StartDate.AddDays(2).Date;
                var permission = new Permission
                {
                    Reason = "Test dentro del cutoff",
                    StartDate = date1,
                    EndDate = date2,
                    Days = (int)((date2 - date1).TotalDays + 1),
                    CutoffId = activeCutoff.Id,
                    PermissionTypeId = 2,
                    RequestingUserId = user.Id,
                    CreatedByUserId = user.Id,
                    CreatedAt = DateTime.Now,
                    PermissionStatusTypeId = (int)PermissionStatusEnum.Pending,
                    WithPay = false
                };
                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
                return Ok(new AlertResponseDTO("Permission", $"Permission creado (ID: {permission.Id})"));
            }

            return Ok(new AlertResponseDTO { Success = false, Title = "No Test", Message = "Caso no reconocido." });
        }
    }
}
