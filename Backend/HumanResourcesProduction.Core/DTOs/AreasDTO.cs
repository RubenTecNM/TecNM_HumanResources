namespace HumanResourcesProduction.Models.DTOs
{
    public class AreasDTO
    {
        public int Id { get; set; }

        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool Status { get; set; }

        public int CreatedByUserId { get; set; }

        public int? UpdatedByUserId { get; set; }

        public virtual User CreatedByUser { get; set; } = null!;

        public virtual User? UpdatedByUser { get; set; } = null!;
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
