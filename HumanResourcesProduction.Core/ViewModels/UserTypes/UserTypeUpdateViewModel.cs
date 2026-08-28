using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels.UserTypes
{
    public class UserTypeUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Description { get; set; }
    }
}
