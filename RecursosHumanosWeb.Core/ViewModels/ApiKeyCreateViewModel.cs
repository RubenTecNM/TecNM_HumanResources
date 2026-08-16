using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RecursosHumanosWeb.Models.ViewModels
{
    public class ApiKeyCreateViewModel
    {
        // -----------------------------------------------------------------
        // PROPIEDADES DE EDICIÓN (Cargadas desde la DB y pasadas como hidden)
        // -----------------------------------------------------------------
        public int Id { get; set; } 

        [Display(Name = "Clave")]
        public string? Key { get; set; }

        [Display(Name = "Fecha Creación")]
        public DateTime CreatedAt { get; set; } 
                                                 

        // Propiedad para el Id del user que crea/modifica (se llena en el controlador)
        public int CreatedByUserId { get; set; }

        // Campo para la selección del user titular (se puede cambiar en EDIT)
        [Display(Name = "User Titular")]
        public int? OwnerUserId { get; set; }

        // Colección para capturar los permissions/tablas seleccionadas (tanto en CREATE como en EDIT)
        public List<ApiPermissionAssignmentViewModel>? Assignments { get; set; } = new List<ApiPermissionAssignmentViewModel>();
    }

  
}