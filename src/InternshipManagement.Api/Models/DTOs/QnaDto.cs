namespace InternshipManagement.Api.Models.DTOs
{
    public class QnaDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? Answer { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AskDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class AnswerDto
    {
        public string Answer { get; set; } = string.Empty;
    }
}
