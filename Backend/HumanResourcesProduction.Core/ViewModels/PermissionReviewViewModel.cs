using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels
{
    public class PermissionReviewViewModel
    {
        // Identificador del permission
        public int Id { get; set; }

        // Información del solicitante (solo lectura)
        public string NombreSolicitante { get; set; } = string.Empty;
        public string CorreoSolicitante { get; set; } = string.Empty;
        public string AreaSolicitante { get; set; } = string.Empty;

        // Información del permission (solo lectura)
        public int PermissionTypeId { get; set; }
        public string PermissionTypeDescription { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Days { get; set; }
        public string? Reason { get; set; }
        public string? Evidence { get; set; }

        // Campo EDITABLE: WithPay de sueldo
        [Required(ErrorMessage = "Debe indicar si el permiso es con o sin goce de sueldo.")]
        public bool WithPay { get; set; }

        // Comentario opcional del autorizador al revisar la solicitud
        [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        public string? AuthorizationComment { get; set; }

        // Información adicional (solo lectura)
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public int CutoffId { get; set; }

        // Información del cutoff (solo lectura)
        public DateTime? CorteInicia { get; set; }
        public DateTime? CorteTermina { get; set; }
    }
}
