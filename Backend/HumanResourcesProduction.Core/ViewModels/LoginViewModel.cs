using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [RegularExpression(@"^[^@\s]+@monclova\.tecnm\.mx$",
         ErrorMessage = "El correo debe pertenecer al dominio monclova.tecnm.mx")]
        [Display(Name = "Email Electr&oacute;nico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contrase&ntilde;a es obligatoria")]
        [Display(Name = "Contrase&ntilde;a")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
