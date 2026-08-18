using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        // Evidencia opcional (PDF) enviada por multipart/form-data
        public IFormFile? EvidenceFile { get; set; }

        // Confirmación de exceder el periodo (segundo intento tras el diálogo)
        public bool? Confirmed { get; set; }
    }
}
