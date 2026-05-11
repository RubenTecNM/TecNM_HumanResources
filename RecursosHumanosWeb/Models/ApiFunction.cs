using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecursosHumanosWeb.Models;

[Table("ApiFunction")]
public partial class ApiFunction : BaseEntity
{
    /// <summary>
    /// Nombre del endpoint/función de la API. Máximo 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "La descripción de la función API es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Descripcion { get; set; } = null!;
}
