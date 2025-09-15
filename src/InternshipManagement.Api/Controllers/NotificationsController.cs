using InternshipManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificationsController(AppDbContext db) => _db = db;


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var now = DateTime.UtcNow;

            var notifications = await _db.Notifications
                .Include(n => n.Meeting)
                
                .Where(n => n.Meeting.ScheduledAt >= now.AddMinutes(-30))
                .OrderByDescending(n => n.NotifyAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.MeetingId,
                    MeetingLink = n.Meeting.MeetingLink,
                    ScheduledAt = n.Meeting.ScheduledAt,
                    n.NotifyAt,
                    n.IsSent,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        [Authorize(Roles = "Intern")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyNotifications()
        {
            var now = DateTime.UtcNow;

            var notifications = await _db.Notifications
                .Include(n => n.Meeting)
                .Where(n =>
                    // meeting in recent window
                    n.Meeting.ScheduledAt >= now.AddMinutes(-30)
                    &&
                    // either already sent OR the notify time has arrived
                    (n.IsSent || n.NotifyAt <= now)
                )
                .OrderByDescending(n => n.NotifyAt)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.MeetingId,
                    MeetingLink = n.Meeting.MeetingLink,
                    ScheduledAt = n.Meeting.ScheduledAt,
                    n.NotifyAt,
                    n.IsSent,
                    n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
