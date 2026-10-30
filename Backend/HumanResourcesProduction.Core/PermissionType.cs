using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("PermissionType")]
public partial class PermissionType : BaseEntity
{
    /// <summary>
    /// Permission type name (Absence, Tardiness, Schedule, Shift). Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del tipo de permiso es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
