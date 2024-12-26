using Microsoft.AspNetCore.Identity;

namespace WebSitem.Models
{
    public class AppUser: IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // Depolama alanı (bytes cinsinden)
        public long StorageLimit { get; set; } = 10L * 1024 * 1024 * 1024; // 10GB
        public long UsedStorage { get; set; } = 0; // Kullanılan alan
    }
}
