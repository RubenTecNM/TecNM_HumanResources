namespace RecursosHumanosWeb.Models.DTOs
{
    public class PermissionsDTO
    {
        public int Id { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? Days { get; set; }

        public string Reason { get; set; }

        public string? Evidence { get; set; }

        public DateTime? AuthorizedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int PermissionStatusTypeId { get; set; }
        public string StatusDescription { get; set; }
        public string StatusColor { get; set; }

        public bool WithPay { get; set; }

        public bool Status { get; set; }

        public int CutoffId { get; set; }

        public int PermissionTypeId { get; set; }

        public int RequestingUserId { get; set; }

        public int? AuthorizingUserId { get; set; }

        public int CreatedByUserId { get; set; }

        public int? UpdatedByUserId { get; set; }

        // Datos del user solicitante
        public string Name { get; set; }

        public string Email { get; set; }

        // Datos adicionales para visualización
        public string PermissionType { get; set; }

        public string Area { get; set; }

        public string? AuthorizedBy { get; set; }

        public string CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
