using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("TipoUsuario")]
public partial class TipoUsuario : BaseEntity
{
    /// <summary>
    /// Nombre del rol/tipo de usuario (Administrador, RecursosHumanos, Trabajador). Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del tipo de usuario es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
