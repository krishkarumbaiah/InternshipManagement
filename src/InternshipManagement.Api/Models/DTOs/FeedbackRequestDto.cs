namespace InternshipManagement.Api.Models.DTOs
{
    public class FeedbackRequestDto
    {
        public string Comments { get; set; } = string.Empty;
        public int Rating { get; set; }
    }

    public class FeedbackResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTimeOffset SubmittedAt { get; set; }
    }
}
