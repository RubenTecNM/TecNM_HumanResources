using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("DatabaseTable")]
public partial class DatabaseTable : BaseEntity
{
    /// <summary>
    /// Table name in the database. Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción de la tabla es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLLECTIONS
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiKeyTablePermission> ApiKeyTablePermissions { get; set; } = new List<ApiKeyTablePermission>();
}
