using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Permisos
{
    public class PermisoApiCreateViewModel
    {
        [Required]
        public string Motivo { get; set; }

        [Required]
        public DateTime Fecha1 { get; set; }

        public DateTime? Fecha2 { get; set; }

        [Required]
        public int IdTipoPermiso { get; set; }

        public bool Goce { get; set; }

        // El usuario solicitante se suele inferir, pero lo permitimos setear por si la API es usada por un sistema maestro
        public int? IdUsuarioSolicita { get; set; }

        // Evidencia por ahora no se incluye o podría ser base64
    }
}
