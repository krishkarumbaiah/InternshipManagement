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

        // 🔹 Admin can view all notifications
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var now = DateTime.UtcNow;

            var notifications = await _db.Notifications
                .Include(n => n.Meeting)
                .ThenInclude(m => m.Batch)
                .Where(n => n.Meeting.ScheduledAt >= now.AddMinutes(-30))
                .OrderByDescending(n => n.NotifyAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.MeetingId,
                    MeetingLink = n.Meeting.MeetingLink,
                    
                    ScheduledAt = n.Meeting.ScheduledAt.ToLocalTime(),
                    BatchName = n.Meeting.Batch != null ? n.Meeting.Batch.Name : "",
                    NotifyAt = n.NotifyAt.ToLocalTime(),
                    n.IsSent,
                    CreatedAt = n.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [Authorize(Roles = "Intern")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);

            var batchIds = await _db.UserBatches
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.BatchId)
                .ToListAsync();

            if (!batchIds.Any())
                return Ok(new List<object>());

            var now = DateTime.UtcNow;

            var notifications = await _db.Notifications
                .Include(n => n.Meeting)
                .ThenInclude(m => m.Batch)
                .Where(n =>
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
                    MeetingLink = n.Meeting.MeetingLink,
                    
                    ScheduledAt = n.Meeting.ScheduledAt.ToLocalTime(),
                    BatchName = n.Meeting.Batch != null ? n.Meeting.Batch.Name : "",
                    NotifyAt = n.NotifyAt.ToLocalTime(),
                    n.IsSent,
                    CreatedAt = n.CreatedAt.ToLocalTime()
                })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
