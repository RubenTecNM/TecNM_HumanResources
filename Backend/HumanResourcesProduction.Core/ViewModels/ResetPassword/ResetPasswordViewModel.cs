using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels.ResetPassword
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El correo electr&oacute;nico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es v&aacute;lido.")]
        [Display(Name = "Email Electr&oacute;nico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contrase&ntilde;a es obligatoria.")]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "La contrase&ntilde;a debe tener entre 6 y 12 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contrase&ntilde;a")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmaci&oacute;n de contrase&ntilde;a es obligatoria.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contrase&ntilde;as no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contrase&ntilde;a")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Selector + verifier token parts
        [Required]
        public string Selector { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
