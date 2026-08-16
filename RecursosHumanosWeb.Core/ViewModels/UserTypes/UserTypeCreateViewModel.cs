using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.UserTypes
{
    public class UserTypeCreateViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Description { get; set; }
    }
}
