using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.Dtos;
using System.Security.Claims;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnnouncementsController(AppDbContext context)
        {
            _context = context;
        }

        //  Admin: Create Announcement
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnnouncementCreateDto dto)
        {
            var batch = await _context.Batches.FindAsync(dto.BatchId);
            if (batch == null)
                return NotFound("Batch not found");

            var announcement = new Announcement
            {
                Title = dto.Title,
                Message = dto.Message,
                BatchId = dto.BatchId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Announcement created successfully" });
        }

        // Intern: Get announcements for their batches
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<AnnouncementResponseDto>>> GetMyAnnouncements()
        {
            // Get logged-in userId from JWT/Claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var announcements = await _context.UserBatches
                .Where(ub => ub.UserId == userId)
                .SelectMany(ub => ub.Batch.Announcements)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementResponseDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt,
                    BatchName = a.Batch.Name
                })
                .ToListAsync();

            return Ok(announcements);
        }
        //  Admin: Get all announcements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnnouncementResponseDto>>> GetAll()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Batch)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AnnouncementResponseDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Message = a.Message,
                    CreatedAt = a.CreatedAt,
                    BatchName = a.Batch.Name
                })
                .ToListAsync();

            return Ok(announcements);
        }

        //  Admin: Delete an announcement
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ann = await _context.Announcements.FindAsync(id);
            if (ann == null)
                return NotFound();

            _context.Announcements.Remove(ann);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
    
    
