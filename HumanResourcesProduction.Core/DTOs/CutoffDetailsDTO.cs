namespace HumanResourcesProduction.Models.DTOs
{
    public class CutoffDetailsDTO
    {
        // Información del cutoff
        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CutoffStatusTypeId { get; set; }
        public string? StatusDescription { get; set; }
        public string? StatusColor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Información de users
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Lista completa de permissions con todos sus datos
        public List<PermissionsDTO> Permissions { get; set; } = new List<PermissionsDTO>();

        // KPIs calculados
        public int TotalPermissions { get; set; }
        public int ApprovedPermissions { get; set; }
        public int PendingPermissions { get; set; }
        public double AverageDays { get; set; }

        // Datos para gráficos (serializados como JSON en el controlador)
        public string PermissionsByTypeJson { get; set; } = "[]";
        public string PermissionsByAreaJson { get; set; } = "[]";
        public string TrendJson { get; set; } = "[]";
        public string HeatmapDataJson { get; set; } = "[]";
        public string KpiDataJson { get; set; } = "{}";
        public int WithPay { get; set; }
        public int WithoutPay { get; set; }
    }
}
