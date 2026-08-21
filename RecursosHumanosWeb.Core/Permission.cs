using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Permission")]
public partial class Permission : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // PERMISSION INFORMATION
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Start date of the requested absence period.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date of the requested absence period.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Number of absence days calculated. Null if not yet determined.
    /// </summary>
    [Range(0, 365, ErrorMessage = "El número de días debe estar entre 0 y 365.")]
    public int? Days { get; set; }

    /// <summary>
    /// Justification or description of the permission reason.
    /// Up to 255 characters (free text field for the employee).
    /// </summary>
    [Required(ErrorMessage = "El motivo del permiso es obligatorio.")]
    [StringLength(255, ErrorMessage = "El motivo no puede superar los 255 caracteres.")]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// URL or path to the attached evidence file. Maximum 255 characters.
    /// </summary>
    [StringLength(255)]
    public string? Evidence { get; set; }

    // ──────────────────────────────────────────────────────────────
    // TEACHER ABSENCE FIELDS (PermissionTypeId = 5)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Work shift FK → ShiftType. 1=Morning, 2=Afternoon, 3=Mixed.
    /// Only used by Teacher Absence type (PermissionTypeId=5).
    /// </summary>
    public int? Shift { get; set; }

    /// <summary>
    /// Hours the teacher was present teaching (in front of classroom).
    /// Only used by Teacher Absence type.
    /// </summary>
    public int? HoursInClassroom { get; set; }

    /// <summary>
    /// Non-teaching hours (tutoring, admin, research). Part of contractual workload.
    /// Only used by Teacher Absence type.
    /// </summary>
    public int? HoursDischarge { get; set; }

    // ──────────────────────────────────────────────────────────────
    // STATUS AND AUTHORIZATION
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Current permission status. FK → PermissionStatusType.
    /// </summary>
    [Required]
    public int PermissionStatusTypeId { get; set; }

    /// <summary>
    /// Indicates whether the permission was approved with pay (true) or without pay (false).
    /// </summary>
    [Required]
    public bool WithPay { get; set; }

    /// <summary>
    /// Date and time when a Department Head authorized or rejected the permission.
    /// </summary>
    public DateTime? AuthorizedAt { get; set; }

    /// <summary>
    /// Optional comment left by the reviewer when authorizing or rejecting the permission.
    /// </summary>
    [StringLength(500, ErrorMessage = "El comentario de autorización no puede superar los 500 caracteres.")]
    public string? AuthorizationComment { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PERMISSION-SPECIFIC FOREIGN KEYS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → Cutoff. Cutoff period this permission belongs to.
    /// </summary>
    [Required]
    public int CutoffId { get; set; }

    /// <summary>
    /// FK → PermissionType. Permission category (Absence, Tardiness, Schedule, Shift).
    /// </summary>
    [Required]
    public int PermissionTypeId { get; set; }

    /// <summary>
    /// FK → User. Employee who requests the permission.
    /// </summary>
    [Required]
    public int RequestingUserId { get; set; }

    /// <summary>
    /// FK → User. Department Head who authorized or rejected the permission. Null if not yet reviewed.
    /// </summary>
    public int? AuthorizingUserId { get; set; }

    // ──────────────────────────────────────────────────────────────
    // CALCULATED PROPERTY (not mapped to a column)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Indicates whether the permission has an attached evidence file. Calculated in memory.
    /// </summary>
    [NotMapped]
    public bool HasEvidence => !string.IsNullOrEmpty(Evidence);

    // ──────────────────────────────────────────────────────────────
    // SPECIFIC NAVIGATION PROPERTIES
    // ──────────────────────────────────────────────────────────────
    public virtual Cutoff Cutoff { get; set; } = null!;
    public virtual PermissionStatusType PermissionStatusType { get; set; } = null!;
    public virtual PermissionType PermissionType { get; set; } = null!;
    public virtual ShiftType? ShiftType { get; set; }
    public virtual User? AuthorizingUser { get; set; }
    public virtual User RequestingUser { get; set; } = null!;
}
