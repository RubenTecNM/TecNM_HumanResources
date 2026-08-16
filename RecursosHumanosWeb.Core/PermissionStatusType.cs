using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("PermissionStatusType")]
public partial class PermissionStatusType : BaseEntity
{
    /// <summary>
    /// Permission status name (Pending, Approved, Rejected, etc.). Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del estado es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// CSS or Bootstrap color to represent the status visually. Up to 20 characters.
    /// </summary>
    [StringLength(20)]
    public string? Color { get; set; }

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
