using System;

namespace RecursosHumanosWeb.Models.ViewModels.User
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int UserTypeId { get; set; }
        public string TipoUsuarioNombre { get; set; } = string.Empty;

        public int AreaId { get; set; }
        public string AreaDescripcion { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public string DepartamentoDescripcion { get; set; } = string.Empty;

        public int PositionId { get; set; }
        public string PuestoDescripcion { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }
        public string? UsuarioCreaNombre { get; set; }

        public int? UpdatedByUserId { get; set; }
        public string? UsuarioModificaNombre { get; set; }

        public int? ApiKeyId { get; set; }

        // Estadísticas de permissions
        public int TotalPermisos { get; set; }
        public int PermisosRevisados { get; set; }
        public int WithPay { get; set; }
        public int SinGoce { get; set; }
        
        // Campos compatibles con el servicio
        public string UserType { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}