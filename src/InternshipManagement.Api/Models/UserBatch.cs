using InternshipManagement.Api.Models;   

namespace InternshipManagement.Api.Models
{
    public class UserBatch
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; } = null;
        public int BatchId { get; set; }
        public Batch? Batch { get; set; } = null;
    }
}
