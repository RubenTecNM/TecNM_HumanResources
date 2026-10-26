namespace HumanResourcesProduction.Core.Dtos;

/// <summary>
/// Configuración de un tipo de permiso para el formulario de creación.
/// Incluye la configuración visual y campos que se deben mostrar.
/// </summary>
public class PermissionTypeCreateConfigDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public PermissionTypeFieldConfigDto Date1 { get; set; } = new();
    public PermissionTypeFieldConfigDto Date2 { get; set; } = new();
    public PermissionTypeFieldConfigDto Time1 { get; set; } = new();
    public PermissionTypeFieldConfigDto Time2 { get; set; } = new();
}

/// <summary>
/// Configuración de campo individual para el DTO de creación.
/// </summary>
public class PermissionTypeFieldConfigDto
{
    public bool Show { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool Required { get; set; }
}

/// <summary>
/// Configuración completa para crear un permiso.
/// El backend es la única autoridad sobre qué tipos puede ver el usuario,
/// qué corte está activo y si puede elegir solicitante.
/// </summary>
public class PermissionCreateConfigDto
{
    public List<PermissionTypeCreateConfigDto> AvailableTypes { get; set; } = new();
    public bool CanChooseRequester { get; set; }
    public int? ActiveCutoffId { get; set; }
    public DateTime? ActiveCutoffStartDate { get; set; }
    public DateTime? ActiveCutoffEndDate { get; set; }
    public int MaxFileSizeMB { get; set; }
    public string[] AllowedEvidenceExtensions { get; set; } = Array.Empty<string>();
}
