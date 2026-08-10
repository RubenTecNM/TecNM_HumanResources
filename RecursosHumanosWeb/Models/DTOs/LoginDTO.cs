using Microsoft.EntityFrameworkCore;

namespace RecursosHumanosWeb.Models.DTOs
{
    [Keyless]
    public class LoginDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public int UserTypeId { get; set; }
    }
}
