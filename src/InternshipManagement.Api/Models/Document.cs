namespace InternshipManagement.Api.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;     
        public string FilePath { get; set; } = null!;       
        public string ContentType { get; set; } = "application/octet-stream";
        public string UploadedBy { get; set; } = "Unknown";
        public DateTimeOffset UploadedAt { get; set; }
    }
}
