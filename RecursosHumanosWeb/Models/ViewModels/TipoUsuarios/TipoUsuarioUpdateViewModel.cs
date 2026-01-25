using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.TipoUsuarios
{
    public class TipoUsuarioUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Descripcion { get; set; }
    }
}
