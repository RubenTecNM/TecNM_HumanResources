using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels.Positions
{
    public class PositionUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La clave tabular es obligatoria.")]
        [StringLength(10, ErrorMessage = "La clave tabular no puede exceder los 10 caracteres.")]
        public string TabularCode { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Description { get; set; }
    }
}
