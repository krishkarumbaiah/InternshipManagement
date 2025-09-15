using InternshipManagement.Api.Data; 
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MeetingsController(AppDbContext db)
        {
            _db = db;
        }

        //  GET: api/meetings (all meetings)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetings = await _db.Meetings
                .OrderByDescending(m => m.ScheduledAt)
                .Select(m => new MeetingDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    ScheduledAt = m.ScheduledAt,
                    MeetingLink = m.MeetingLink,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(meetings);
        }

        // GET: api/meetings/upcoming
        [Authorize]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeetings()
        {
            var now = DateTime.UtcNow;
            var meetings = await _db.Meetings
                .Where(m => m.ScheduledAt >= now)
                .OrderBy(m => m.ScheduledAt)
                .Select(m => new MeetingDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    ScheduledAt = m.ScheduledAt,
                    MeetingLink = m.MeetingLink,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(meetings);
        }

       // POST: api/meetings
[Authorize(Roles = "Admin")]
[HttpPost]
public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var meeting = new Meeting
    {
        Title = dto.Title,
        Description = dto.Description,
        ScheduledAt = dto.ScheduledAt,
        MeetingLink = dto.MeetingLink
    };

    _db.Meetings.Add(meeting);
    await _db.SaveChangesAsync();

    //  Auto-create notification for 15 mins before
    var notifyTime = dto.ScheduledAt.AddMinutes(-15);
    var notification = new Notification
    {
        MeetingId = meeting.Id,
        NotifyAt = notifyTime,
        Message = $"Reminder: Meeting '{meeting.Title}' starts at {dto.ScheduledAt:g}.",
        IsSent = false
    };
    _db.Notifications.Add(notification);
    await _db.SaveChangesAsync();

    return Ok(new { message = "Meeting created successfully", meeting.Id });
}


        // DELETE: api/meetings/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _db.Meetings.FindAsync(id);
            if (meeting == null) return NotFound("Meeting not found");

            _db.Meetings.Remove(meeting);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Meeting deleted successfully" });
        }
    }
}
