using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Position")]
public partial class Position : BaseEntity
{
    /// <summary>
    /// Descriptive position name. Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción del puesto es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Tabular reference identifier for integrations. Up to 20 characters.
    /// </summary>
    [Required(ErrorMessage = "La clave tabular es obligatoria.")]
    [StringLength(20, ErrorMessage = "La clave tabular no puede superar los 20 caracteres.")]
    public string TabularCode { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
