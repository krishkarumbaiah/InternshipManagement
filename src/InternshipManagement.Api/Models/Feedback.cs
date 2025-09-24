using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Api.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  

        [Required]
        [MaxLength(1000)]
        public string Comments { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }  

        public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}
