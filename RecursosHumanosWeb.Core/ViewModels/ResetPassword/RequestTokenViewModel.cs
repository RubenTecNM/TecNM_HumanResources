using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.ResetPassword
{
    public class RequestTokenViewModel
    {
        [Required(ErrorMessage = "El correo electr&oacute;nico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es v&aacute;lido.")]
        [RegularExpression(@"^[^@\s]+@monclova\.tecnm\.mx$", ErrorMessage = "El correo debe pertenecer al dominio monclova.tecnm.mx")]
        [Display(Name = "Email Electr&oacute;nico")]
        public string Email { get; set; } = string.Empty;
    }
}
