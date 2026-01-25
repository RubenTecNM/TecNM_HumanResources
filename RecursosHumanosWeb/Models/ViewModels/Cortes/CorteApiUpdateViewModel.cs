using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Cortes
{
    public class CorteApiUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime Inicia { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime Termina { get; set; }

        public bool Estatus { get; set; }
    }
}
