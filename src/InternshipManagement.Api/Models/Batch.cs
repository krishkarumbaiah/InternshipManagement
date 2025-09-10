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
    }
}
