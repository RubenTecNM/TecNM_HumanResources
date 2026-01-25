using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Cortes
{
    public class CorteApiCreateViewModel
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime Inicia { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime Termina { get; set; }
    }
}
