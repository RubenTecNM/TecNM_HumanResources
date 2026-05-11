using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("ApiKey")]
public partial class ApiKey : BaseEntity
{
    /// <summary>
    /// Clave de autenticación de la API. SHA-256 produce exactamente 64 caracteres hexadecimales.
    /// </summary>
    [Required(ErrorMessage = "La clave de API es obligatoria.")]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "La clave debe ser un hash SHA-256 de exactamente 64 caracteres.")]
    public string Clave { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiPermisosApiKeysTabla> ApiPermisosApiKeysTablas { get; set; } = new List<ApiPermisosApiKeysTabla>();
}
