namespace RecursosHumanosWeb.Models.ViewModels
{
    // -----------------------------------------------------------------
    // SUB-MODELO: Detalle de Asignación de Permissions por DatabaseTable
    // -----------------------------------------------------------------
    public class ApiPermissionAssignmentViewModel
    {
        // ID de la DatabaseTable a la que se le asignarán permissions (ej. 1=User, 2=DatabaseTable)
        public int TableId { get; set; }

        // Lista de IDs de los ApiPermissions seleccionados (ej. [1, 2] = Leer, Escribir)
        // El Mvc Binder recolecta los valores de todos los checkboxes marcados aquí.
        public List<int>? IdsApiPermissionsSeleccionados { get; set; }
    }
}
