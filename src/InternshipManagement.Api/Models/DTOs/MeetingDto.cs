using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models.DTOs
{
    public class CreateMeetingDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Required]
        public string MeetingLink { get; set; } = string.Empty;
        [Required]
        public int BatchId { get; set; }
    }

    public class MeetingDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string MeetingLink { get; set; } = string.Empty;

        
        public DateTime CreatedAt { get; set; }
    }
}
