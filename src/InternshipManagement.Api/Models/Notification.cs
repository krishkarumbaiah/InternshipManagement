using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;

        //  Add BatchId and navigation
        public int BatchId { get; set; }
        public Batch? Batch { get; set; }

        public DateTime NotifyAt { get; set; } // 15 mins before meeting
        public string Message { get; set; } = string.Empty;
        public bool IsSent { get; set; } = false; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
