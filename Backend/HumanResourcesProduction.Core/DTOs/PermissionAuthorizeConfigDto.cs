namespace HumanResourcesProduction.Core.Dtos;

/// <summary>
/// Configuración para autorizar (aprobar/rechazar) un permiso existente.
/// El backend es la única autoridad sobre quién puede autorizar y qué opciones tiene.
/// </summary>
public class PermissionAuthorizeConfigDto
{
    public int Id { get; set; }
    public int PermissionTypeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Requester { get; set; } = string.Empty;
    public int RequesterId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public bool WithPay { get; set; }
    public int CutoffId { get; set; }
    public string CutoffRange { get; set; } = string.Empty;
    public bool CanAuthorize { get; set; }
    public string? BlockReason { get; set; }
    public bool CanChangeWithPay { get; set; }
    public List<WithPayOptionDto> WithPayOptions { get; set; } = new();
}

/// <summary>
/// Opción de Con/Sin goce para autorización.
/// </summary>
public class WithPayOptionDto
{
    public bool Value { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
