using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HumanResourcesProduction.Services;

namespace HumanResourcesProduction.Features.Sync;

[Route("api/Sync")]
[Authorize(Policy = "RHOrAdmin")]
public class SyncApiController : ControllerBase
{
    private readonly ExcelImportService _importService;
    private readonly ILogger<SyncApiController> _logger;

    public SyncApiController(ExcelImportService importService, ILogger<SyncApiController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    private int ActorId => int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 1;

    /// <summary>Descarga la plantilla Excel con hoja de datos y leyenda.</summary>
    [HttpGet("template/{kind}")]
    public async Task<IActionResult> Template(string kind)
    {
        _logger.LogInformation("[ExcelSync] Usuario {UserId} descargó plantilla de tipo '{Kind}'", ActorId, kind);
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
        {
            _logger.LogWarning("[ExcelSync] Intento de validación con archivo vacío por usuario {UserId}", ActorId);
            return BadRequest(new { message = "Adjunte un archivo .xlsx." });
        }
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[ExcelSync] Intento de validación con formato no .xlsx ('{FileName}') por usuario {UserId}", file.FileName, ActorId);
            return BadRequest(new { message = "El archivo debe ser .xlsx." });
        }

        try
        {
            _logger.LogInformation("[ExcelSync] Usuario {UserId} validó Excel '{FileName}' (Tipo: {Kind}, {Size} bytes)",
                ActorId, file.FileName, kind, file.Length);
            using var stream = file.OpenReadStream();
            var preview = await _importService.ValidateAsync(kind, stream);
            _logger.LogInformation("[ExcelSync] Resultado de validación '{FileName}': {Total} total, {New} nuevos, {Dup} duplicados, {Err} errores",
                file.FileName, preview.TotalRows, preview.NewCount, preview.DuplicateCount, preview.ErrorCount);
            return Ok(preview);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "[ExcelSync] Error de validación en Excel '{FileName}' por usuario {UserId}: {Message}", file.FileName, ActorId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExcelSync] Excepción no controlada durante validación de Excel '{FileName}' por usuario {UserId}", file.FileName, ActorId);
            throw;
        }
    }

    /// <summary>Ejecuta la carga: crea nuevos, omite duplicados y reporta errores por fila.</summary>
    [HttpPost("import/{kind}")]
    public async Task<IActionResult> Import(string kind, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("[ExcelSync] Intento de importación con archivo vacío por usuario {UserId}", ActorId);
            return BadRequest(new { message = "Adjunte un archivo .xlsx." });
        }
        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[ExcelSync] Intento de importación con formato no .xlsx ('{FileName}') por usuario {UserId}", file.FileName, ActorId);
            return BadRequest(new { message = "El archivo debe ser .xlsx." });
        }

        try
        {
            _logger.LogInformation("[ExcelSync] Usuario {UserId} inició importación '{FileName}' (Tipo: {Kind}, {Size} bytes)",
                ActorId, file.FileName, kind, file.Length);
            using var stream = file.OpenReadStream();
            var summary = await _importService.ImportAsync(kind, stream, ActorId);
            _logger.LogInformation("[ExcelSync] Importación completada '{FileName}': Creados={Created}, Duplicados={Dup}, Errores={Err}",
                file.FileName,
                summary.CreatedUsers + summary.PromotedAuthorizers + summary.CreatedCutoffs + summary.CreatedPermissions,
                summary.Duplicates,
                summary.Errors.Count);
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "[ExcelSync] Error de negocio al importar '{FileName}' por usuario {UserId}: {Message}", file.FileName, ActorId, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ExcelSync] Excepción no controlada durante importación de Excel '{FileName}' por usuario {UserId}", file.FileName, ActorId);
            throw;
        }
    }
}
