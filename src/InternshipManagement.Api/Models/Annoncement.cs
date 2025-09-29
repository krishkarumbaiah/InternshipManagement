using System;

namespace InternshipManagement.Api.Models
{
    public class Announcement
    {
        public int Id { get; set; }  // Primary key
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Relationship with Batch
        public int BatchId { get; set; }
        public Batch Batch { get; set; } = null!;
    }
}
