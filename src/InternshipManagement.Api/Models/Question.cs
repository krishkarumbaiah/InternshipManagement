using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Api.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Answer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? BatchId { get; set; }
        [ForeignKey(nameof(BatchId))]
        public Batch? Batch { get; set; }
    }
}
