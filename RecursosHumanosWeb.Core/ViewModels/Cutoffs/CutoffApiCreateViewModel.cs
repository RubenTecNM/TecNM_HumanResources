using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Cutoffs
{
    public class CutoffApiCreateViewModel
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; }
    }
}
