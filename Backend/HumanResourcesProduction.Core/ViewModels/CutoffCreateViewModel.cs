using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels
{
    public class CutoffCreateViewModel
    {
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
