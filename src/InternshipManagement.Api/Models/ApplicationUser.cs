using Microsoft.AspNetCore.Identity;

namespace InternshipManagement.Api.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public string? ProfilePhotoPath { get; set; }
    }
}
