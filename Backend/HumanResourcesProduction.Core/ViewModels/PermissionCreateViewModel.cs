using System.ComponentModel.DataAnnotations;

namespace HumanResourcesProduction.Models.ViewModels
{
    /// <summary>
    /// ViewModel exclusivo para el flujo de creación de permisos.
    /// No expone campos que solo deben modificarse en edición/revisión
    /// (WithPay, Id, Evidence) para evitar over-posting (mass assignment).
    /// </summary>
    public class PermissionCreateViewModel
    {
        // El motivo es siempre obligatorio y tiene un límite de longitud
        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(255, ErrorMessage = "El motivo no puede exceder los 255 caracteres.")]
        public string Reason { get; set; } = null!;

        // StartDate: Almacena la fecha y hora combinadas (inicio o evento principal)
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        // EndDate: Almacena la fecha y hora combinadas (fin o evento secundario, si aplica)
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        // El tipo de permiso es siempre obligatorio
        [Required(ErrorMessage = "El tipo de permiso es obligatorio.")]
        public int PermissionTypeId { get; set; }

        // Archivo de evidencia (IFormFile): Es opcional, por lo que ya es nullable
        public IFormFile? EvidenceFile { get; set; }

        // Campos temporales para la UI (no se guardan en la base de datos)
        [DataType(DataType.Date)]
        public DateTime? StartDateOnly { get; set; }

        [DataType(DataType.Time)]
        public string? StartTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDateOnly { get; set; }

        [DataType(DataType.Time)]
        public string? EndTime { get; set; }

        // User que solicita (opcional, prellenado en creación)
        public int? RequestingUserId { get; set; }

        // User que crea (opcional, prellenado en creación)
        public int? CreatedByUserId { get; set; }
    }
}
