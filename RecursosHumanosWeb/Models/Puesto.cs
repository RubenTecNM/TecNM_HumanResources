using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Puesto")]
public partial class Puesto : BaseEntity
{
    /// <summary>
    /// Nombre descriptivo del puesto. Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del puesto es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Identificador tabular de referencia para integraciones. Máximo 20 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La clave tabular es obligatoria.")]
    [StringLength(20, ErrorMessage = "La clave tabular no puede superar los 20 caracteres.")]
    public string ClaveTabular { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
