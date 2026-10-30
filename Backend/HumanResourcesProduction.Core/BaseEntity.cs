using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

/// <summary>
/// Abstract base class for all domain entities.
/// Centralizes the audit fields and status control, avoiding duplication
/// and guaranteeing consistency across all tables.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Auto-incremental primary key.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Indicates whether the record is active (true) or was soft-deleted (false).
    /// </summary>
    [Required]
    public bool Status { get; set; } = true;

    // ──────────────────────────────────────────────────────────────
    // AUDIT FIELDS - CREATION
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Date and time when the record was created. Assigned automatically.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// ID of the user who created the record. Foreign key to User.
    /// </summary>
    [Required]
    public int CreatedByUserId { get; set; }

    // ──────────────────────────────────────────────────────────────
    // AUDIT FIELDS - MODIFICATION
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Date and time of the last modification. Null if never modified.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ID of the user who performed the last modification. Null if never modified.
    /// </summary>
    public int? UpdatedByUserId { get; set; }

    // ──────────────────────────────────────────────────────────────
    // NAVIGATION PROPERTIES
    // Defined as virtual to allow lazy loading.
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Navigation to the user who created the record.
    /// </summary>
    public virtual User? CreatedByUser { get; set; }

    /// <summary>
    /// Navigation to the user who last modified the record.
    /// </summary>
    public virtual User? UpdatedByUser { get; set; }
}
