using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("OriginType")]
public partial class OriginType : BaseEntity
{
    /// <summary>
    /// Name of the audit operation origin (WebApp, API, AutomaticSystem). Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del origen es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
