namespace InternshipManagement.Api.Models.DTOs
{
    public class UserBatchDto
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public int BatchId { get; set; }

        // Optional extras for display
        public string? UserName { get; set; }
        public string? BatchName { get; set; }
    }
}
