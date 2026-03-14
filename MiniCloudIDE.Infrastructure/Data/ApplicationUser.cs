using Microsoft.AspNetCore.Identity;

namespace MiniCloudIDE.Infrastructure.Data
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
