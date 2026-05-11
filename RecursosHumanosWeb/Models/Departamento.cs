using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Departamento")]
public partial class Departamento : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo del departamento. Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del departamento es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
