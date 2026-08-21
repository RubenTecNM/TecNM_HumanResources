using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("ShiftType")]
public partial class ShiftType : BaseEntity
{
    /// <summary>
    /// Shift type name (Morning, Afternoon, Mixed). Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del tipo de turno es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
