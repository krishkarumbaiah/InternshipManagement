using InternshipManagement.Api.Models;

namespace InternshipManagement.Api.Models
{
    public class Batch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ICollection<UserBatch> UserBatches { get; set; } = new List<UserBatch>();
        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
        public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();

    }
}
