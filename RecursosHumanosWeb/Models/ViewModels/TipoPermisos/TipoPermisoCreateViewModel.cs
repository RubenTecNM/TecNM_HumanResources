using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.TipoPermisos
{
    public class TipoPermisoCreateViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Descripcion { get; set; }
    }
}
