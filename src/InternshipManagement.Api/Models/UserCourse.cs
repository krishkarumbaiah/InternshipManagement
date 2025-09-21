namespace InternshipManagement.Api.Models
{
    public class UserCourse
    {
        public int Id { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;  

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;        

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    }
}
