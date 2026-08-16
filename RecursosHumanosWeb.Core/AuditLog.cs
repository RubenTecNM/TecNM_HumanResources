using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("AuditLog")]
public partial class AuditLog : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // FOREIGN KEYS AND TRACEABILITY
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → DatabaseTable. Table the audited record belongs to.
    /// </summary>
    [Required]
    public int TableId { get; set; }

    /// <summary>
    /// ID of the specific record affected in the audited table.
    /// </summary>
    [Required]
    public int RecordId { get; set; }

    /// <summary>
    /// FK → ApiFunction. Endpoint or function that generated the event.
    /// </summary>
    [Required]
    public int ApiFunctionId { get; set; }

    /// <summary>
    /// FK → ApiKey. API key used, if the operation came from the external API. Optional.
    /// </summary>
    public int? ApiKeyId { get; set; }

    /// <summary>
    /// FK → OriginType. Indicates whether it was WebApp, API or an automatic system.
    /// </summary>
    [Required]
    public int OriginTypeId { get; set; }

    /// <summary>
    /// JSON snapshot of the data BEFORE the change so it can be rolled back or audited.
    /// No strict length limit to avoid losing audit data.
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? PreviousData { get; set; }

    // ──────────────────────────────────────────────────────────────
    // SPECIFIC NAVIGATION PROPERTIES
    // ──────────────────────────────────────────────────────────────
    public virtual ApiFunction ApiFunction { get; set; } = null!;
    public virtual ApiKey? ApiKey { get; set; }
    public virtual OriginType OriginType { get; set; } = null!;
    public virtual DatabaseTable Table { get; set; } = null!;
}
