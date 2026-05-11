using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Permiso")]
public partial class Permiso : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // INFORMACIÓN DEL PERMISO
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fecha de inicio del período de ausencia solicitado.
    /// </summary>
    public DateTime? Fecha1 { get; set; }

    /// <summary>
    /// Fecha de fin del período de ausencia solicitado.
    /// </summary>
    public DateTime? Fecha2 { get; set; }

    /// <summary>
    /// Número de días de ausencia calculados. Null si aún no se ha determinado.
    /// </summary>
    [Range(0, 365, ErrorMessage = "El número de días debe estar entre 0 y 365.")]
    public int? Dias { get; set; }

    /// <summary>
    /// Justificación o descripción del motivo del permiso.
    /// Hasta 255 caracteres (campo de texto libre para el trabajador).
    /// </summary>
    [Required(ErrorMessage = "El motivo del permiso es obligatorio.")]
    [StringLength(255, ErrorMessage = "El motivo no puede superar los 255 caracteres.")]
    public string Motivo { get; set; } = null!;

    /// <summary>
    /// URL o ruta al archivo de evidencia adjunta. Máximo 255 caracteres.
    /// </summary>
    [StringLength(255)]
    public string? Evidencia { get; set; }

    // ──────────────────────────────────────────────────────────────
    // ESTADO Y AUTORIZACIÓN
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Estado actual del permiso. FK → TipoEstadoPermiso.
    /// </summary>
    [Required]
    public int IdEstadoPermiso { get; set; }

    /// <summary>
    /// Indica si el permiso fue aprobado con goce de sueldo (true) o sin goce (false).
    /// </summary>
    [Required]
    public bool Goce { get; set; }

    /// <summary>
    /// Fecha y hora en que un Jefe de Área autorizó o rechazó el permiso.
    /// </summary>
    public DateTime? FechaAutorizacion { get; set; }

    // ──────────────────────────────────────────────────────────────
    // LLAVES FORÁNEAS ESPECÍFICAS DEL PERMISO
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → Corte. Período de corte al que pertenece este permiso.
    /// </summary>
    [Required]
    public int IdCorte { get; set; }

    /// <summary>
    /// FK → TipoPermiso. Categoría del permiso (Falta, Retardo, Horario, Turno).
    /// </summary>
    [Required]
    public int IdTipoPermiso { get; set; }

    /// <summary>
    /// FK → Usuario. Empleado que solicita el permiso.
    /// </summary>
    [Required]
    public int IdUsuarioSolicita { get; set; }

    /// <summary>
    /// FK → Usuario. Jefe de Área que autorizó o rechazó el permiso. Null si aún no fue revisado.
    /// </summary>
    public int? IdUsuarioAutoriza { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDAD CALCULADA (no se mapea a columna)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Indica si el permiso tiene adjunto de evidencia. Calculado en memoria.
    /// </summary>
    [NotMapped]
    public bool TieneEvidencia => !string.IsNullOrEmpty(Evidencia);

    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NAVEGACIÓN ESPECÍFICAS
    // ──────────────────────────────────────────────────────────────
    public virtual Corte IdCorteNavigation { get; set; } = null!;
    public virtual TipoEstadoPermiso IdEstadoPermisoNavigation { get; set; } = null!;
    public virtual TipoPermiso IdTipoPermisoNavigation { get; set; } = null!;
    public virtual Usuario? IdUsuarioAutorizaNavigation { get; set; }
    public virtual Usuario IdUsuarioSolicitaNavigation { get; set; } = null!;
}
