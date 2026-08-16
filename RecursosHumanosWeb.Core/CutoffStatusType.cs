using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("CutoffStatusType")]
public partial class CutoffStatusType : BaseEntity
{
    /// <summary>
    /// Cutoff status name (Open, Closed, Invalidated). Up to 50 characters.
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
    public virtual ICollection<Cutoff> Cutoffs { get; set; } = new List<Cutoff>();
}
