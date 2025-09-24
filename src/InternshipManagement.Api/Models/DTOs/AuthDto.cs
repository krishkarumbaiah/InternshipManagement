using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InternshipManagement.Api.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        
        public string? Role { get; set; }

        // OTP required by your flow
        [Required]
        public string Otp { get; set; } = string.Empty;

        // File upload from multipart/form-data
        public IFormFile? ProfilePhoto { get; set; }
    }

    public record LoginDto(
        [Required] string UserName,
        [Required] string Password
    );
}
