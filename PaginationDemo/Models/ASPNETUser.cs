using Microsoft.AspNetCore.Identity;

namespace PaginationDemo.Models
{
    public class ASPNETUser : IdentityUser
    {
        public List<string> roles { get; set; } = null!;
    }
}
