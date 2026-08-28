namespace HumanResourcesProduction.Models.DTOs
{
    public class UsersDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? TempPassword { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Status { get; set; }

        public int UserTypeId { get; set; }

        public int AreaId { get; set; }

        public int DepartmentId { get; set; }

        public int PositionId { get; set; }

        public int CreatedByUserId { get; set; }

        public int UpdatedByUserId { get; set; }

        public int? ApiKeyId { get; set; }
    }
}
