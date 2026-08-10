using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Department")]
public partial class Department : BaseEntity
{
    /// <summary>
    /// Descriptive department name. Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del departamento es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
