using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HumanResourcesProduction.Models;

[Table("ApiFunction")]
public partial class ApiFunction : BaseEntity
{
    /// <summary>
    /// Name of the API endpoint/function. Up to 50 characters.
    /// </summary>
    [Required(ErrorMessage = "La descripción de la función API es obligatoria.")]
    [StringLength(50, ErrorMessage = "La descripción no puede superar los 50 caracteres.")]
    public string Description { get; set; } = null!;
}
