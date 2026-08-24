using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Departments
{
    public class DepartmentUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Description { get; set; }

        /// <summary>
        /// Optional area this department belongs to. Must reference an existing active area when provided.
        /// </summary>
        public int? AreaId { get; set; }
    }
}
