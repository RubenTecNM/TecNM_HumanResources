using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Area")]
public partial class Area : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo del área organizacional. Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del área es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
