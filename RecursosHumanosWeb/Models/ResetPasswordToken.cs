using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

/// <summary>
/// Entidad de dominio que representa un token temporal para restablecer contraseñas.
/// </summary>
[Table("ResetPasswordToken")]
public class ResetPasswordToken : BaseEntity
{
    /// <summary>
    /// Parte pública del token enviada en el URL (no secreta por sí sola).
    /// Máximo 20 caracteres: identificador corto para la búsqueda eficiente.
    /// </summary>
    [Required]
    [StringLength(20, ErrorMessage = "El selector no puede superar los 20 caracteres.")]
    public string Selector { get; set; } = string.Empty;

    /// <summary>
    /// Hash SHA-256 de la parte secreta del token. Exactamente 64 caracteres hex.
    /// </summary>
    [Required]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "El hash del token debe ser SHA-256 (64 caracteres).")]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el token ya fue utilizado (true) para prevenir reutilización.
    /// </summary>
    [Required]
    public bool Utilizado { get; set; } = false;

    /// <summary>
    /// FK → Usuario. Usuario que solicitó el restablecimiento de contraseña.
    /// </summary>
    [Required]
    public int IdUsuarioSolicita { get; set; }

    /// <summary>
    /// Fecha y hora en que el token expira y deja de ser válido.
    /// </summary>
    [Required]
    public DateTime FechaCaducidad { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDAD CALCULADA (no se mapea a columna)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Indica si el token ya ha vencido. Calculado en memoria.
    /// </summary>
    [NotMapped]
    public bool EstaVencido => DateTime.Now > FechaCaducidad;

    /// <summary>
    /// Indica si el token es válido: no usado, activo y no vencido. Calculado en memoria.
    /// </summary>
    [NotMapped]
    public bool EsValido => !Utilizado && Estatus && !EstaVencido;
}
