namespace InternshipManagement.Api.Models.Dtos
{
    public class AnnouncementCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int BatchId { get; set; }
    }

    public class AnnouncementResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public string BatchName { get; set; } = string.Empty;
    }
}
