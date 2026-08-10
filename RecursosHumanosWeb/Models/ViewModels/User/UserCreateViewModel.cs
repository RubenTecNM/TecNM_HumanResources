using System.ComponentModel.DataAnnotations;

namespace RecursosHumanosWeb.Models.ViewModels.User
{
    // Define el modelo de datos que se espera recibir en el cuerpo (Body) de la petición POST.
    public class UserCreateViewModel
    {
        // --- Propiedades requeridas para la creación ---

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder los 100 caracteres.")]
        [Display(Name = "Name Completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(150, ErrorMessage = "El correo no debe exceder los 150 caracteres.")]
        [Display(Name = "Email Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "La contraseña debe tener entre 4 y 50 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        // --- Claves foráneas necesarias (IDs) ---

        [Display(Name = "Tipo de User")]
        [Required(ErrorMessage = "El Tipo de Usuario es obligatorio.")]
        public int UserTypeId { get; set; }

        [Display(Name = "Department")]
        [Required(ErrorMessage = "El Departamento es obligatorio.")]
        public int DepartmentId { get; set; }

        [Display(Name = "Área")]
        [Required(ErrorMessage = "El Área es obligatoria.")]
        public int AreaId { get; set; }

        [Display(Name = "Position")]
        [Required(ErrorMessage = "El Puesto es obligatorio.")]
        public int PositionId { get; set; }

        // Nota: ApiKeyId se elimina completamente de aquí.
        // Las API Keys se administran a través del controlador ApiKeysController
        // después de que el user ha sido creado.

        // Nota: Los campos de auditoría (CreatedAt, CreatedByUserId, Status) 
        // son manejados por el controlador y no se incluyen aquí.
    }
}