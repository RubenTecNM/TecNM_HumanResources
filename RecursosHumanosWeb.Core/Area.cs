using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Area")]
public partial class Area : BaseEntity
{
    /// <summary>
    /// Descriptive name of the organizational area. Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del área es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
