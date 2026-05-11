using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("Usuario")]
public partial class Usuario : BaseEntity
{
    // ──────────────────────────────────────────────────────────────
    // INFORMACIÓN PERSONAL Y DE ACCESO
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Nombre completo del empleado. Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Correo electrónico. Se usa como nombre de usuario de inicio de sesión.
    /// Debe ser único en la base de datos.
    /// </summary>
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Correo { get; set; } = null!;

    /// <summary>
    /// Contraseña almacenada como hash SHA-256.
    /// SHA-256 produce siempre exactamente 64 caracteres hexadecimales.
    /// </summary>
    [Required(ErrorMessage = "La clave es obligatoria.")]
    [StringLength(64, MinimumLength = 64, ErrorMessage = "La clave debe ser un hash SHA-256 de exactamente 64 caracteres.")]
    public string Clave { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // POSICIÓN ORGANIZACIONAL - LLAVES FORÁNEAS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// FK → TipoUsuario. Rol del usuario en el sistema.
    /// </summary>
    [Required]
    public int IdTipoUsuario { get; set; }

    /// <summary>
    /// FK → Area. Área organizacional a la que pertenece el usuario.
    /// </summary>
    [Required]
    public int IdArea { get; set; }

    /// <summary>
    /// FK → Departamento. Departamento al que pertenece el usuario.
    /// </summary>
    [Required]
    public int IdDepartamento { get; set; }

    /// <summary>
    /// FK → Puesto. Puesto o cargo del usuario.
    /// </summary>
    [Required]
    public int IdPuesto { get; set; }

    /// <summary>
    /// FK → ApiKey (opcional). Clave de API asignada al usuario para acceso programático.
    /// </summary>
    public int? IdApiKey { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NAVEGACIÓN ESPECÍFICAS
    // (La herencia de BaseEntity ya provee IdUsuarioCreaNavigation e IdUsuarioModificaNavigation,
    //  pero Usuario se auto-referencia, así que los sobrescribimos de forma nullable.)
    // ──────────────────────────────────────────────────────────────
    public virtual Area? IdAreaNavigation { get; set; }
    public virtual Departamento? IdDepartamentoNavigation { get; set; }
    public virtual Puesto? IdPuestoNavigation { get; set; }
    public virtual TipoUsuario? IdTipoUsuarioNavigation { get; set; }

    // Auto-referencia: El creador/modificador también es un Usuario.
    // Se usa '?' para evitar ciclos de dependencia al cargar el Id=1 (root).
    public new virtual Usuario? IdUsuarioCreaNavigation { get; set; }
    public new virtual Usuario? IdUsuarioModificaNavigation { get; set; }

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES DE REGISTROS DONDE ESTE USUARIO ES AUDITOR
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<ApiFunction> ApiFunctionIdUsuarioCreaNavigations { get; set; } = new List<ApiFunction>();
    public virtual ICollection<ApiFunction>? ApiFunctionIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<ApiKey> ApiKeyIdUsuarioCreaNavigations { get; set; } = new List<ApiKey>();
    public virtual ICollection<ApiKey>? ApiKeyIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<ApiPermiso> ApiPermisoIdUsuarioCreaNavigations { get; set; } = new List<ApiPermiso>();
    public virtual ICollection<ApiPermiso>? ApiPermisoIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<ApiPermisosApiKeysTabla> ApiPermisosApiKeysTablaIdUsuarioCreaNavigations { get; set; } = new List<ApiPermisosApiKeysTabla>();
    public virtual ICollection<ApiPermisosApiKeysTabla>? ApiPermisosApiKeysTablaIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<Area> AreaIdUsuarioCreaNavigations { get; set; } = new List<Area>();
    public virtual ICollection<Area>? AreaIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<Corte> CorteIdUsuarioCreaNavigations { get; set; } = new List<Corte>();
    public virtual ICollection<Corte>? CorteIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<Departamento> DepartamentoIdUsuarioCreaNavigations { get; set; } = new List<Departamento>();
    public virtual ICollection<Departamento>? DepartamentoIdUsuarioModificaNavigations { get; set; }

    // Colección inversa de la auto-referencia de auditoría
    public virtual ICollection<Usuario> InverseIdUsuarioCreaNavigation { get; set; } = new List<Usuario>();
    public virtual ICollection<Usuario>? InverseIdUsuarioModificaNavigation { get; set; }

    public virtual ICollection<Permiso>? PermisoIdUsuarioAutorizaNavigations { get; set; }
    public virtual ICollection<Permiso> PermisoIdUsuarioCreaNavigations { get; set; } = new List<Permiso>();
    public virtual ICollection<Permiso>? PermisoIdUsuarioModificaNavigations { get; set; }
    public virtual ICollection<Permiso> PermisoIdUsuarioSolicitaNavigations { get; set; } = new List<Permiso>();

    public virtual ICollection<Puesto> PuestoIdUsuarioCreaNavigations { get; set; } = new List<Puesto>();
    public virtual ICollection<Puesto>? PuestoIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<Tabla> TablaIdUsuarioCreaNavigations { get; set; } = new List<Tabla>();
    public virtual ICollection<Tabla>? TablaIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<TipoPermiso> TipoPermisoIdUsuarioCreaNavigations { get; set; } = new List<TipoPermiso>();
    public virtual ICollection<TipoPermiso>? TipoPermisoIdUsuarioModificaNavigations { get; set; }

    public virtual ICollection<TipoUsuario> TipoUsuarioIdUsuarioCreaNavigations { get; set; } = new List<TipoUsuario>();
    public virtual ICollection<TipoUsuario>? TipoUsuarioIdUsuarioModificaNavigations { get; set; }
}