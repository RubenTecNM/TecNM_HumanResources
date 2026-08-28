using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels.ResetPassword
{
    public class ConfirmTokenViewModel
    {
        [Required(ErrorMessage = "El correo electr&oacute;nico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es v&aacute;lido.")]
        [Display(Name = "Email Electr&oacute;nico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token de seguridad es obligatorio.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "El token debe tener entre 6 y 20 caracteres.")]
        [Display(Name = "Token de Seguridad")]
        public string Token { get; set; } = string.Empty;
    }
}
