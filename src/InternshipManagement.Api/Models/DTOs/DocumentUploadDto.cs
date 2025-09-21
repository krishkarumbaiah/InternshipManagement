using Microsoft.AspNetCore.Http;

namespace InternshipManagement.Api.Models.DTOs
{
    public class DocumentUploadDto
    {
        public IFormFile File { get; set; } = null!;
    }
}
