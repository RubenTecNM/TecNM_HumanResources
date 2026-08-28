using System;

namespace HumanResourcesProduction.Models.DTOs
{
    public class AuditLogDTO
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public string DatabaseTable { get; set; }
        public string Accion { get; set; }
        public string? ApiKeyDescription { get; set; }
        public int OriginTypeId { get; set; }
        public string? OriginTypeDescription { get; set; }
        public string? PreviousData { get; set; }
        public DateTime CreatedAt { get; set; }
        public string User { get; set; }
    }
}
