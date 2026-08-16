using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.Permissions
{
    public class PermissionApiCreateViewModel
    {
        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public int PermissionTypeId { get; set; }

        public bool WithPay { get; set; }

        // El user solicitante se suele inferir, pero lo permitimos setear por si la API es usada por un sistema maestro
        public int? RequestingUserId { get; set; }

        // Evidence por ahora no se incluye o podría ser base64
    }
}
