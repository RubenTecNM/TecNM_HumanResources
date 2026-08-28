using System;

namespace HumanResourcesProduction.Models.ViewModels.User
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
        public string UserTypeName { get; set; } = string.Empty;

        public int AreaId { get; set; }
        public string AreaDescription { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public string DepartmentDescription { get; set; } = string.Empty;

        public int PositionId { get; set; }
        public string PositionDescription { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }
        public string? CreatedByName { get; set; }

        public int? UpdatedByUserId { get; set; }
        public string? UpdatedByName { get; set; }

        public int? ApiKeyId { get; set; }

        public string? TempPassword { get; set; }

        // Estadísticas de permissions
        public int TotalPermissions { get; set; }
        public int ReviewedPermissions { get; set; }
        public int WithPay { get; set; }
        public int WithoutPay { get; set; }
        
        // Campos compatibles con el servicio
        public string UserType { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}