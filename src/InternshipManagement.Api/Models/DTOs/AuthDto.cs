using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models.DTOs
{
    public record RegisterDto(
        [Required] string UserName,
        [Required, EmailAddress] string Email,
        [Required] string FullName,
        [Required] string Password,
        string? Role
    );

    public record LoginDto(
        [Required] string UserName,
        [Required] string Password
    );
}
