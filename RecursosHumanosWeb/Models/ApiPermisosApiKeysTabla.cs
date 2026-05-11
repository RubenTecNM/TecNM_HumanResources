using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("ApiPermisosApiKeysTabla")]
public partial class ApiPermisosApiKeysTabla : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // LLAVES FORÁNEAS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → ApiKey. Asocia esta entrada con una clave de API específica.
    /// </summary>
    [Required]
    public int IdApiKey { get; set; }

    /// <summary>
    /// FK → ApiPermiso. Tipo de permiso concedido por esta clave.
    /// </summary>
    [Required]
    public int IdApiPermiso { get; set; }

    /// <summary>
    /// FK → Tabla. Tabla de la DB sobre la que aplica el permiso.
    /// </summary>
    [Required]
    public int IdTabla { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NAVEGACIÓN ESPECÍFICAS
    // ──────────────────────────────────────────────────────────────
    public virtual ApiKey IdApiKeyNavigation { get; set; } = null!;
    public virtual ApiPermiso IdApiPermisoNavigation { get; set; } = null!;
    public virtual Tabla IdTablaNavigation { get; set; } = null!;
}
