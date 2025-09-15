using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Api.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }   // FK → ApplicationUser

        [Required]
        public int BatchId { get; set; }  // FK → Batch

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsPresent { get; set; }

        // Navigation
        public ApplicationUser? User { get; set; }
        public Batch? Batch { get; set; }
    }
}
