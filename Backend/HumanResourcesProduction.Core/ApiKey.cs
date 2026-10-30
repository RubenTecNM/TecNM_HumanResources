using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("ApiKey")]
public partial class ApiKey : BaseEntity
{
    /// <summary>
    /// API authentication key. SHA-256 produces exactly 64 hexadecimal characters.
    /// </summary>
    [Required(ErrorMessage = "La clave de API es obligatoria.")]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "La clave debe ser un hash SHA-256 de exactamente 64 caracteres.")]
    public string Key { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiKeyTablePermission> ApiKeyTablePermissions { get; set; } = new List<ApiKeyTablePermission>();
}
