using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("ApiKeyTablePermission")]
public partial class ApiKeyTablePermission : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // FOREIGN KEYS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → ApiKey. Associates this entry with a specific API key.
    /// </summary>
    [Required]
    public int ApiKeyId { get; set; }

    /// <summary>
    /// FK → ApiPermission. Permission type granted by this key.
    /// </summary>
    [Required]
    public int ApiPermissionId { get; set; }

    /// <summary>
    /// FK → DatabaseTable. DB table this permission applies to.
    /// </summary>
    [Required]
    public int TableId { get; set; }

    // ──────────────────────────────────────────────────────────────
    // SPECIFIC NAVIGATION PROPERTIES
    // ──────────────────────────────────────────────────────────────
    public virtual ApiKey ApiKey { get; set; } = null!;
    public virtual ApiPermission ApiPermission { get; set; } = null!;
    public virtual DatabaseTable Table { get; set; } = null!;
}
