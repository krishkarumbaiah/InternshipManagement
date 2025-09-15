using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models.DTOs
{
    public record AttendanceDto
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string? UserName { get; init; }
        public int BatchId { get; init; }
        public string? BatchName { get; init; }
        public DateTime Date { get; init; }
        public bool IsPresent { get; init; }
    }

    public record MarkAttendanceDto(
        [Required] int UserId,
        [Required] int BatchId,
        [Required] DateTime Date,
        [Required] bool IsPresent
    );
}
