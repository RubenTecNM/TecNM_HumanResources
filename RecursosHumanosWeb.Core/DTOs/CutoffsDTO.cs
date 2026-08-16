namespace RecursosHumanosWeb.Models.DTOs
{
    public class CutoffsDTO
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int CutoffStatusTypeId { get; set; }
        public string StatusDescription { get; set; }
        public string StatusColor { get; set; }

        public int CreatedByUserId { get; set; }

        public int? UpdatedByUserId { get; set; }
    }
}
