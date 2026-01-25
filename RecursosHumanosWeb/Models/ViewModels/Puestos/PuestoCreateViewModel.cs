using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Puestos
{
    public class PuestoCreateViewModel
    {
        [Required(ErrorMessage = "La clave tabular es obligatoria.")]
        [StringLength(10, ErrorMessage = "La clave tabular no puede exceder los 10 caracteres.")]
        public string ClaveTabular { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Descripcion { get; set; }
    }
}
