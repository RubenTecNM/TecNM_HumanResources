using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("TipoOrigen")]
public partial class TipoOrigen : BaseEntity
{
    /// <summary>
    /// Nombre del origen de la operación de auditoría (WebApp, API, SistemaAutomatico). Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción del origen es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;

    // ──────────────────────────────────────────────────────────────
    // COLECCIONES
    // ──────────────────────────────────────────────────────────────
    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();
}
