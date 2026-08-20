using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels
{
    public class PermissionsCreateViewModel
    {
        // Identificador único del permission (opcional, solo para edición)
        public int? Id { get; set; }

        // El motivo es siempre obligatorio y tiene un límite de longitud
        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(255, ErrorMessage = "El motivo no puede exceder los 255 caracteres.")]
        public string Reason { get; set; }

        // StartDate: Almacena la fecha y hora combinadas (inicio o evento principal)
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }

        // EndDate: Almacena la fecha y hora combinadas (fin o evento secundario, si aplica)
        [DataType(DataType.DateTime)]
        public DateTime? EndDate { get; set; }

        // El tipo de permission es siempre obligatorio
        [Required(ErrorMessage = "El tipo de permiso es obligatorio.")]
        public int PermissionTypeId { get; set; }

        // Ruta de la evidencia (string): Puede ser nula, longitud limitada
        [StringLength(255, ErrorMessage = "La evidencia no puede exceder los 255 caracteres.")]
        public string? Evidence { get; set; }

        // Archivo de evidencia (IFormFile): Es opcional, por lo que ya es nullable
        public IFormFile? EvidenceFile { get; set; }

        // WithPay de sueldo (opcional, editable solo por Administrator/Authorizer en edición)
        public bool? WithPay { get; set; }

        // Campos temporales para la UI (no se guardan en la base de datos)
        [DataType(DataType.Date)]
        public DateTime? StartDateOnly { get; set; }

        // Cambiar TimeSpan a string para compatibilidad con input type="time"
        [DataType(DataType.Time)]
        public string? StartTime { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDateOnly { get; set; }

        [DataType(DataType.Time)]
        public string? EndTime { get; set; }

        // User que solicita (opcional, prellenado en creación, de solo lectura en edición)
        public int? RequestingUserId { get; set; }

        // User que crea (opcional, prellenado en creación)
        public int? CreatedByUserId { get; set; }

        // Teacher Absence fields (PermissionTypeId = 5)
        public int? Shift { get; set; }
        public int? HoursInClassroom { get; set; }
        public int? HoursDischarge { get; set; }
    }
}
