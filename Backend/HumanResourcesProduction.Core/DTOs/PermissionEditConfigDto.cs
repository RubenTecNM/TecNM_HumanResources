namespace HumanResourcesProduction.Core.Dtos;

/// <summary>
/// Configuración para editar un permiso existente.
/// El backend es la única autoridad sobre si se puede editar y por qué.
/// </summary>
public class PermissionEditConfigDto
{
    public int Id { get; set; }
    public int PermissionTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
    public string? BlockReason { get; set; }
    public int? ActiveCutoffId { get; set; }
    public DateTime? ActiveCutoffStartDate { get; set; }
    public DateTime? ActiveCutoffEndDate { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Reason { get; set; }
    public bool WithPay { get; set; }
    public int HoursInClassroom { get; set; }
    public int HoursDischarge { get; set; }
    public string EvidenceUrl { get; set; } = string.Empty;
    public int MaxFileSizeMB { get; set; }
    public string[] AllowedEvidenceExtensions { get; set; } = Array.Empty<string>();
    public List<PermissionTypeEditConfigDto> AvailableTypes { get; set; } = new();
}

/// <summary>
/// Tipo de permiso disponible para edición.
/// </summary>
public class PermissionTypeEditConfigDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public PermissionTypeFieldConfigDto Date1 { get; set; } = new();
    public PermissionTypeFieldConfigDto Date2 { get; set; } = new();
    public PermissionTypeFieldConfigDto Time1 { get; set; } = new();
    public PermissionTypeFieldConfigDto Time2 { get; set; } = new();
}
