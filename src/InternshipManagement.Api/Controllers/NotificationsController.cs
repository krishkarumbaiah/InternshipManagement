using InternshipManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InternshipManagement.Api.Models;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

       
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var now = DateTime.UtcNow;


            var notifications = await _db.Notifications
                .AsNoTracking()
                .Include(n => n.Meeting)
                    .ThenInclude(m => m.Batch)
                .Where(n => n.MeetingId != null && n.Meeting != null &&
                            n.Meeting.ScheduledAt >= now.AddMinutes(-30))
                .OrderByDescending(n => n.NotifyAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.MeetingId,
                    MeetingLink = n.Meeting != null ? n.Meeting.MeetingLink : string.Empty,
                    ScheduledAt = n.Meeting != null ? n.Meeting.ScheduledAt.ToLocalTime() : (DateTime?)null,
                    BatchName = n.Meeting != null && n.Meeting.Batch != null ? n.Meeting.Batch.Name : string.Empty,
                    NotifyAt = n.NotifyAt.ToLocalTime(),
                    n.IsSent,
                    CreatedAt = n.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // Intern: get meeting notifications only for batches user belongs to
        [Authorize(Roles = "Intern")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            // Get user's batches
            var batchIds = await _db.UserBatches
                .AsNoTracking()
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.BatchId)
                .ToListAsync();

            if (!batchIds.Any())
                return Ok(new List<object>());

            var now = DateTime.UtcNow;

            
            var notifications = await _db.Notifications
                .AsNoTracking()
                .Include(n => n.Meeting)
                    .ThenInclude(m => m.Batch)
                .Where(n =>
                    n.MeetingId != null &&                       
                    n.Meeting != null &&
                    batchIds.Contains(n.BatchId) &&              
                    n.Meeting.ScheduledAt >= now.AddMinutes(-30) &&
                    (n.IsSent || n.NotifyAt <= now)
                )
                .OrderByDescending(n => n.NotifyAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.MeetingId,
                    MeetingLink = n.Meeting != null ? n.Meeting.MeetingLink : string.Empty,
                    ScheduledAt = n.Meeting != null ? n.Meeting.ScheduledAt.ToLocalTime() : (DateTime?)null,
                    BatchName = n.Meeting != null && n.Meeting.Batch != null ? n.Meeting.Batch.Name : string.Empty,
                    NotifyAt = n.NotifyAt.ToLocalTime(),
                    n.IsSent,
                    CreatedAt = n.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
