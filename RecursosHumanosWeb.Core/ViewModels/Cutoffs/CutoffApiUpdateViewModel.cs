using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Cutoffs
{
    public class CutoffApiUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime EndDate { get; set; }

        public bool Status { get; set; }
    }
}
