using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("User")]
public partial class User : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // PERSONAL AND ACCESS INFORMATION
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Full employee name. Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Email address. Used as the login user name.
    /// Must be unique in the database.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Password stored as a SHA-256 hash.
    /// SHA-256 always produces exactly 64 hexadecimal characters.
    /// </summary>
    [Required(ErrorMessage = "La clave es obligatoria.")]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "La clave debe ser un hash SHA-256 de exactamente 64 caracteres.")]
    public string PasswordHash { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // ORGANIZATIONAL POSITION - FOREIGN KEYS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → UserType. User role in the system.
    /// </summary>
    [Required]
    public int UserTypeId { get; set; }

    /// <summary>
    /// FK → Area. Organizational area the user belongs to.
    /// </summary>
    [Required]
    public int AreaId { get; set; }

    /// <summary>
    /// FK → Department. Department the user belongs to.
    /// </summary>
    [Required]
    public int DepartmentId { get; set; }

    /// <summary>
    /// FK → Position. User position or job title.
    /// </summary>
    [Required]
    public int PositionId { get; set; }

    /// <summary>
    /// FK → ApiKey (optional). API key assigned to the user for programmatic access.
    /// </summary>
    public int? ApiKeyId { get; set; }

    /// <summary>
    /// Plain-text temporary password issued by RH (import or reset).
    /// Visible to RH until the user logs in with it, then cleared.
    /// </summary>
    public string? TempPassword { get; set; }

    // ──────────────────────────────────────────────────────────────
    // SPECIFIC NAVIGATION PROPERTIES
    // (BaseEntity already provides CreatedByUser and UpdatedByUser,
    //  but User is self-referencing, so we override them as nullable.)
    // ──────────────────────────────────────────────────────────────
    public virtual Area? Area { get; set; }
    public virtual Department? Department { get; set; }
    public virtual Position? Position { get; set; }
    public virtual UserType? UserType { get; set; }

    // Self-reference: the creator/modifier is also a User.
    // '?' is used to avoid dependency cycles when loading Id=1 (root).
    public new virtual User? CreatedByUser { get; set; }
    public new virtual User? UpdatedByUser { get; set; }

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS OF RECORDS WHERE THIS USER IS THE AUDITOR
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiFunction> ApiFunctionCreatedBy { get; set; } = new List<ApiFunction>();
    public virtual ICollection<ApiFunction>? ApiFunctionUpdatedBy { get; set; }

    public virtual ICollection<ApiKey> ApiKeyCreatedBy { get; set; } = new List<ApiKey>();
    public virtual ICollection<ApiKey>? ApiKeyUpdatedBy { get; set; }

    public virtual ICollection<ApiPermission> ApiPermissionCreatedBy { get; set; } = new List<ApiPermission>();
    public virtual ICollection<ApiPermission>? ApiPermissionUpdatedBy { get; set; }

    public virtual ICollection<ApiKeyTablePermission> ApiKeyTablePermissionCreatedBy { get; set; } = new List<ApiKeyTablePermission>();
    public virtual ICollection<ApiKeyTablePermission>? ApiKeyTablePermissionUpdatedBy { get; set; }

    public virtual ICollection<Area> AreaCreatedBy { get; set; } = new List<Area>();
    public virtual ICollection<Area>? AreaUpdatedBy { get; set; }

    public virtual ICollection<Cutoff> CutoffCreatedBy { get; set; } = new List<Cutoff>();
    public virtual ICollection<Cutoff>? CutoffUpdatedBy { get; set; }

    public virtual ICollection<Department> DepartmentCreatedBy { get; set; } = new List<Department>();
    public virtual ICollection<Department>? DepartmentUpdatedBy { get; set; }

    // Inverse collection of the audit self-reference
    public virtual ICollection<User> InverseCreatedBy { get; set; } = new List<User>();
    public virtual ICollection<User>? InverseUpdatedBy { get; set; }

    public virtual ICollection<Permission>? PermissionAuthorizedBy { get; set; }
    public virtual ICollection<Permission> PermissionCreatedBy { get; set; } = new List<Permission>();
    public virtual ICollection<Permission>? PermissionUpdatedBy { get; set; }
    public virtual ICollection<Permission> PermissionRequestedBy { get; set; } = new List<Permission>();

    public virtual ICollection<Position> PositionCreatedBy { get; set; } = new List<Position>();
    public virtual ICollection<Position>? PositionUpdatedBy { get; set; }

    public virtual ICollection<DatabaseTable> DatabaseTableCreatedBy { get; set; } = new List<DatabaseTable>();
    public virtual ICollection<DatabaseTable>? DatabaseTableUpdatedBy { get; set; }

    public virtual ICollection<PermissionType> PermissionTypeCreatedBy { get; set; } = new List<PermissionType>();
    public virtual ICollection<PermissionType>? PermissionTypeUpdatedBy { get; set; }

    public virtual ICollection<ShiftType> ShiftTypeCreatedBy { get; set; } = new List<ShiftType>();
    public virtual ICollection<ShiftType>? ShiftTypeUpdatedBy { get; set; }

    public virtual ICollection<UserType> UserTypeCreatedBy { get; set; } = new List<UserType>();
    public virtual ICollection<UserType>? UserTypeUpdatedBy { get; set; }
}
