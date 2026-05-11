using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Auditoria")]
public partial class Auditoria : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // LLAVES FORÁNEAS Y TRAZABILIDAD
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → Tabla. Tabla a la que pertenece el registro auditado.
    /// </summary>
    [Required]
    public int IdTabla { get; set; }

    /// <summary>
    /// ID del registro específico que fue afectado en la tabla auditada.
    /// </summary>
    [Required]
    public int IdRegistro { get; set; }

    /// <summary>
    /// FK → ApiFunction. Endpoint o función que generó el evento.
    /// </summary>
    [Required]
    public int IdApiFunction { get; set; }

    /// <summary>
    /// FK → ApiKey. Clave de API usada, si la operación vino de la API externa. Opcional.
    /// </summary>
    public int? IdApiKey { get; set; }

    /// <summary>
    /// FK → TipoOrigen. Indica si fue WebApp, API o sistema automático.
    /// </summary>
    [Required]
    public int IdTipoOrigen { get; set; }

    /// <summary>
    /// Snapshot JSON de los datos ANTES del cambio para poder revertir o auditar.
    /// Sin límite estricto de longitud para no perder datos de auditoría.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? DatosAnteriores { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NAVEGACIÓN ESPECÍFICAS
    // ──────────────────────────────────────────────────────────────
    public virtual ApiFunction IdApiFunctionNavigation { get; set; } = null!;
    public virtual ApiKey? IdApiKeyNavigation { get; set; }
    public virtual TipoOrigen IdTipoOrigenNavigation { get; set; } = null!;
    public virtual Tabla IdTablaNavigation { get; set; } = null!;
}
