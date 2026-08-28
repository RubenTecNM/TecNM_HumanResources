using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels.PermissionTypes
{
    public class PermissionTypeCreateViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Description { get; set; }
    }
}
