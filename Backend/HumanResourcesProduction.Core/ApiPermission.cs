using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("ApiPermission")]
public partial class ApiPermission : BaseEntity
{
    /// <summary>
    /// API permission description (read, write, etc.). Up to 50 characters. Optional.
    /// </summary>
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string? Description { get; set; }

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiKeyTablePermission> ApiKeyTablePermissions { get; set; } = new List<ApiKeyTablePermission>();
}
