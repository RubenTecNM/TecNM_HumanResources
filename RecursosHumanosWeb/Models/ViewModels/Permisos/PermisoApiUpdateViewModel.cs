using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Permisos
{
    public class PermisoApiUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Motivo { get; set; }

        [Required]
        public DateTime Fecha1 { get; set; }

        public DateTime? Fecha2 { get; set; }

        public int IdTipoPermiso { get; set; }
        
        public bool Goce { get; set; }
    }
}
