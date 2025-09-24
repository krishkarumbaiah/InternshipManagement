using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models
{
    public class Meeting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledAt { get; set; }

        [Required]
        public string MeetingLink { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public int BatchId { get; set; }
        public Batch? Batch { get; set; } = null;
    }
}
