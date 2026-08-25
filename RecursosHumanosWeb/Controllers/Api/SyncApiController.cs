using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecursosHumanosWeb.Services;

namespace RecursosHumanosWeb.Controllers.Api;

[Route("api/Sync")]
[Authorize(Policy = "RHOrAdmin")]
public class SyncApiController : ControllerBase
{
    private readonly ExcelImportService _importService;

    public SyncApiController(ExcelImportService importService)
    {
        _importService = importService;
    }

    private int ActorId => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 1;

    /// <summary>Descarga la plantilla Excel con hoja de datos y leyenda.</summary>
    [HttpGet("template/{kind}")]
    public async Task<IActionResult> Template(string kind)
    {
        var file = await _importService.BuildTemplateAsync(kind);
        return File(file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"plantilla_{kind}.xlsx");
    }

    /// <summary>Compara el archivo contra la base de datos sin escribir nada.</summary>
    [HttpPost("validate/{kind}")]
    public async Task<IActionResult> Validate(string kind, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Adjunte un archivo .xlsx." });
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "El archivo debe ser .xlsx." });

        try
        {
            using var stream = file.OpenReadStream();
            var preview = await _importService.ValidateAsync(kind, stream);
            return Ok(preview);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Ejecuta la carga: crea nuevos, omite duplicados y reporta errores por fila.</summary>
    [HttpPost("import/{kind}")]
    public async Task<IActionResult> Import(string kind, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Adjunte un archivo .xlsx." });
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "El archivo debe ser .xlsx." });

        try
        {
            using var stream = file.OpenReadStream();
            var summary = await _importService.ImportAsync(kind, stream, ActorId);
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
