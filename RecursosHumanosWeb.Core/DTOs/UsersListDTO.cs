namespace RecursosHumanosWeb.Models.DTOs
{
    public class UsersListDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Department { get; set; } = string.Empty;
    }
}
