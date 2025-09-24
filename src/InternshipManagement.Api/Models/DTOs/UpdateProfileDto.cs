using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        // Optional profile photo
        public IFormFile? ProfilePhoto { get; set; }
    }
}
