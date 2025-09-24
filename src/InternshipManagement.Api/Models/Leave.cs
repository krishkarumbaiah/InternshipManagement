using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManagement.Api.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }   // ✅ int, matches ApplicationUser.Id

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        // Navigation
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }
    }
}
