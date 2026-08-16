using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Cutoff")]
public partial class Cutoff : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // BUSINESS PROPERTIES
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Start date of the cutoff period.
    /// </summary>
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the cutoff period. Must be later than StartDate.
    /// </summary>
    [Required(ErrorMessage = "La fecha de término es obligatoria.")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Current period status. FK → CutoffStatusType (1=Open, 2=Closed, 3=Invalidated).
    /// </summary>
    [Required]
    public int CutoffStatusTypeId { get; set; }

    // ──────────────────────────────────────────────────────────────
    // CALCULATED PROPERTY (not mapped to a column)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Period duration in days. Calculated in memory.
    /// </summary>
    [NotMapped]
    public int DurationDays => (EndDate - StartDate).Days;

    // ──────────────────────────────────────────────────────────────
    // SPECIFIC NAVIGATION PROPERTIES
    // ──────────────────────────────────────────────────────────────
    public virtual CutoffStatusType CutoffStatusType { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
