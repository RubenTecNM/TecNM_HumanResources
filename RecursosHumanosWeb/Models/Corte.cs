using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Corte")]
public partial class Corte : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NEGOCIO
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fecha de inicio del período de corte.
    /// </summary>
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime Inicia { get; set; }

    /// <summary>
    /// Fecha de término del período de corte. Debe ser posterior a Inicia.
    /// </summary>
    [Required(ErrorMessage = "La fecha de término es obligatoria.")]
    public DateTime Termina { get; set; }

    /// <summary>
    /// Estado actual del período. FK → TipoEstadoCorte (1=Abierto, 2=Cerrado, 3=Invalidado).
    /// </summary>
    [Required]
    public int IdEstadoCorte { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDAD CALCULADA (no se mapea a columna)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Duración del período en días. Calculada en memoria.
    /// </summary>
    [NotMapped]
    public int DuracionDias => (Termina - Inicia).Days;

    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NAVEGACIÓN ESPECÍFICAS
    // ──────────────────────────────────────────────────────────────
    public virtual TipoEstadoCorte IdEstadoCorteNavigation { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
