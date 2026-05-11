using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Tabla")]
public partial class Tabla : BaseEntity
{
    /// <summary>
    /// Nombre de la tabla en la base de datos. Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción de la tabla es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiPermisosApiKeysTabla> ApiPermisosApiKeysTablas { get; set; } = new List<ApiPermisosApiKeysTabla>();
}
