using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

/// <summary>
/// Clase base abstracta para todas las entidades del dominio.
/// Centraliza los campos de auditoría y el control de estado, evitando duplicación
/// y garantizando consistencia en todas las tablas.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Clave primaria autoincremental.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Indica si el registro está activo (true) o fue dado de baja lógica (false).
    /// </summary>
    [Required]
    public bool Estatus { get; set; } = true;

    // ──────────────────────────────────────────────────────────────
    // CAMPOS DE AUDITORÍA - CREACIÓN
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fecha y hora en que se creó el registro. Se asigna automáticamente.
    /// </summary>
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    /// <summary>
    /// ID del usuario que creó el registro. Llave foránea hacia Usuario.
    /// </summary>
    [Required]
    public int IdUsuarioCrea { get; set; }

    // ──────────────────────────────────────────────────────────────
    // CAMPOS DE AUDITORÍA - MODIFICACIÓN
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fecha y hora de la última modificación. Null si nunca se ha modificado.
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// ID del usuario que realizó la última modificación. Null si nunca se ha modificado.
    /// </summary>
    public int? IdUsuarioModifica { get; set; }

    // ──────────────────────────────────────────────────────────────
    // PROPIEDADES DE NAVEGACIÓN
    // Se definen como virtual para permitir lazy loading.
    // Las clases hijas deben configurar el [ForeignKey] en OnModelCreating
    // si EF Core no puede inferirlo automáticamente.
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Navegación hacia el usuario que creó el registro.
    /// </summary>
    public virtual Usuario? IdUsuarioCreaNavigation { get; set; }

    /// <summary>
    /// Navegación hacia el usuario que modificó el registro por última vez.
    /// </summary>
    public virtual Usuario? IdUsuarioModificaNavigation { get; set; }
}
