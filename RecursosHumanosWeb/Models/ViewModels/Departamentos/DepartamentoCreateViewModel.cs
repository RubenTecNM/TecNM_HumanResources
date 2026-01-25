using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Departamentos
{
    public class DepartamentoCreateViewModel
    {
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Descripcion { get; set; }
    }
}
