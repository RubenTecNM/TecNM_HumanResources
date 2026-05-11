using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("TipoPermiso")]
public partial class TipoPermiso : BaseEntity
{
    /// <summary>
    /// Nombre del tipo de permiso (Falta, Retardo, Horario, Turno). Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del tipo de permiso es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
}
