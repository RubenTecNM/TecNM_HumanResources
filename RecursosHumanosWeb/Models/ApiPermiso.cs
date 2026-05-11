using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("ApiPermiso")]
public partial class ApiPermiso : BaseEntity
{
    /// <summary>
    /// Descripción del permiso de API (lectura, escritura, etc.). Máximo 50 caracteres. Opcional.
    /// </summary>
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string? Descripcion { get; set; }

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiPermisosApiKeysTabla> ApiPermisosApiKeysTablas { get; set; } = new List<ApiPermisosApiKeysTabla>();
}
