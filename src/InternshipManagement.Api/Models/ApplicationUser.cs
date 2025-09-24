using Microsoft.AspNetCore.Identity;

namespace InternshipManagement.Api.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
        public string? ProfilePhotoPath { get; set; }

         // ✅ Add these properties for batch assignment
        public int? BatchId { get; set; }
        public Batch? Batch { get; set; }
    }
}
