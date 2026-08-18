using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RecursosHumanosWeb.Models.ViewModels.Permissions
{
    public class PermissionApiUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PermissionTypeId { get; set; }

        public bool WithPay { get; set; }

        // Evidencia opcional (PDF) enviada por multipart/form-data
        public IFormFile? EvidenceFile { get; set; }

        // Si el usuario quita la evidencia existente sin subir una nueva
        public bool? ClearEvidence { get; set; }

        // Confirmación de exceder el periodo (segundo intento tras el diálogo)
        public bool? Confirmed { get; set; }
    }
}