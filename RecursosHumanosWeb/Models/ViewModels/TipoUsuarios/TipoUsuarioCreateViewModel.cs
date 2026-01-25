using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.TipoUsuarios
{
    public class TipoUsuarioCreateViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Descripcion { get; set; }
    }
}
