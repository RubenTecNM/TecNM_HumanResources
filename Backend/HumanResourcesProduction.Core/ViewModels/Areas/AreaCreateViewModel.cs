using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels.Areas
{
    public class AreaCreateViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Description { get; set; }
    }
}
