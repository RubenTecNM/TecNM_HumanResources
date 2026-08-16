using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

/// <summary>
/// Domain entity representing a temporary password reset token.
/// </summary>
[Table("ResetPasswordToken")]
public class ResetPasswordToken : BaseEntity
{
    /// <summary>
    /// Public part of the token sent in the URL (not secret by itself).
    /// Up to 20 characters: short identifier for efficient lookup.
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "El selector no puede superar los 20 caracteres.")]
    public string Selector { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the secret part of the token. Exactly 64 hex characters.
    /// </summary>
    [Required]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "El hash del token debe ser SHA-256 (64 caracteres).")]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the token was already used (true) to prevent reuse.
    /// </summary>
    [Required]
    public bool Used { get; set; } = false;

    /// <summary>
    /// FK → User. User who requested the password reset.
    /// </summary>
    [Required]
    public int RequestingUserId { get; set; }

    /// <summary>
    /// Date and time when the token expires and becomes invalid.
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    // ──────────────────────────────────────────────────────────────
    // CALCULATED PROPERTIES (not mapped to columns)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Indicates whether the token has already expired. Calculated in memory.
    /// </summary>
    [NotMapped]
    public bool IsExpired => DateTime.Now > ExpiresAt;

    /// <summary>
    /// Indicates whether the token is valid: unused, active and not expired. Calculated in memory.
    /// </summary>
    [NotMapped]
    public bool IsValid => !Used && Status && !IsExpired;
}
