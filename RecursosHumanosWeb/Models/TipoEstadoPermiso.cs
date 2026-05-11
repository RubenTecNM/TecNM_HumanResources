using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("TipoEstadoPermiso")]
public partial class TipoEstadoPermiso : BaseEntity
{
    /// <summary>
    /// Nombre del estado del permiso (Pendiente, Aprobado, Rechazado, etc.). Máximo 50 caracteres.
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
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
