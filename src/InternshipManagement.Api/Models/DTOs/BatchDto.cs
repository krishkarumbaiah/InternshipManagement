using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models.DTOs
{
    public class BatchDto
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

    public record CreateBatchDto([Required] string Name, DateTime StartDate, DateTime EndDate);
    public record UpdateBatchDto([Required] string Name, DateTime StartDate, DateTime EndDate);
}
