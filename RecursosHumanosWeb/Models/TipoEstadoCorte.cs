using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("TipoEstadoCorte")]
public partial class TipoEstadoCorte : BaseEntity
{
    /// <summary>
    /// Nombre del estado del corte (Abierto, Cerrado, Invalidado). Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del estado es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Color CSS o Bootstrap para representar el estado visualmente. Máximo 20 caracteres.
    /// </summary>
    [StringLength(20)]
    public string? Color { get; set; }

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Corte> Cortes { get; set; } = new List<Corte>();
}
