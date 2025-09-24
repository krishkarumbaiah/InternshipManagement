using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InternshipManagement.Api.Services;

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
        private static MeetingDto MapToDto(Meeting m)
        {
            
            var scheduledUtc = DateTime.SpecifyKind(m.ScheduledAt, DateTimeKind.Utc);
            var createdUtc = DateTime.SpecifyKind(m.CreatedAt, DateTimeKind.Utc);

            return new MeetingDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                // convert to local time so frontend sees correct local date/time
                ScheduledAt = scheduledUtc.ToLocalTime(),
                MeetingLink = m.MeetingLink,
                CreatedAt = createdUtc.ToLocalTime(),
                BatchId = m.BatchId,
                BatchName = m.Batch != null ? m.Batch.Name : string.Empty
            };
        }

        // ================== ADMIN + COMMON ==================

        // GET: api/meetings (all meetings)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            var meetingsEntities = await _db.Meetings
                .Include(m => m.Batch)
                .OrderByDescending(m => m.ScheduledAt)
                .ToListAsync();

            var meetings = meetingsEntities.Select(MapToDto).ToList();
            return Ok(meetings);
        }

        // GET: api/meetings/upcoming
        [Authorize]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingMeetings()
        {
            var now = DateTime.UtcNow;
            var meetingsEntities = await _db.Meetings
                .Include(m => m.Batch)
                .Where(m => m.ScheduledAt >= now)
                .OrderBy(m => m.ScheduledAt)
                .ToListAsync();

            var meetings = meetingsEntities.Select(MapToDto).ToList();
            return Ok(meetings);
        }

        // POST: api/meetings
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingDto dto,
                                                       [FromServices] IEmailService emailService)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var batch = await _db.Batches
                .Include(b => b.UserBatches)
                .ThenInclude(ub => ub.User)
                .FirstOrDefaultAsync(b => b.Id == dto.BatchId);

            if (batch == null) return NotFound(new { message = "Batch not found" });

            // Save meeting as UTC
            var meeting = new Meeting
            {
                Title = dto.Title,
                Description = dto.Description,
                ScheduledAt = dto.ScheduledAt.Kind == DateTimeKind.Utc
                    ? dto.ScheduledAt
                    : dto.ScheduledAt.ToUniversalTime(),
                MeetingLink = dto.MeetingLink,
                BatchId = dto.BatchId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Meetings.Add(meeting);
            await _db.SaveChangesAsync();

            // Use the stored UTC meeting.ScheduledAt to calculate notify time
            var notifyTimeUtc = meeting.ScheduledAt.AddMinutes(-10); // keep your chosen offset (was -3)
            var notification = new Notification
            {
                MeetingId = meeting.Id,
                BatchId = meeting.BatchId,
                NotifyAt = notifyTimeUtc,
                Message = $"Reminder: Meeting '{meeting.Title}' starts at {meeting.ScheduledAt.ToLocalTime():g}.",
                IsSent = false
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            // Immediately send email to interns in this batch (email shows local time)
            if (batch.UserBatches != null)
            {
                foreach (var ub in batch.UserBatches)
                {
                    if (!string.IsNullOrEmpty(ub.User?.Email))
                    {
                        try
                        {
                            await emailService.SendEmailAsync(
                                ub.User.Email,
                                "Meeting Scheduled",
                                $"Hi {ub.User.FullName ?? "Intern"},<br/>" +
                                $"You have a meeting <b>{meeting.Title}</b> scheduled at {meeting.ScheduledAt.ToLocalTime():g}.<br/>" +
                                $"Link: <a href='{meeting.MeetingLink}'>{meeting.MeetingLink}</a>"
                            );
                        }
                        catch (Exception ex)
                        {
                            // Log failure but don’t block meeting creation
                            Console.WriteLine($"❌ Failed to send email to {ub.User.Email}: {ex.Message}");
                        }
                    }
                }
            }

            return Ok(new { message = "Meeting created and notifications sent", meeting.Id });
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

        // ================== BATCH-SPECIFIC ==================

        // GET: api/meetings/upcoming/{batchId}
        [Authorize]
        [HttpGet("upcoming/{batchId:int}")]
        public async Task<IActionResult> GetUpcomingMeetingsForBatch(int batchId)
        {
            var now = DateTime.UtcNow;
            var meetingsEntities = await _db.Meetings
                .Include(m => m.Batch)
                .Where(m => m.ScheduledAt >= now && m.BatchId == batchId)
                .OrderBy(m => m.ScheduledAt)
                .ToListAsync();

            var meetings = meetingsEntities.Select(MapToDto).ToList();
            return Ok(meetings);
        }

        [Authorize(Roles = "Intern")]
        [HttpGet("upcoming/mine")]
        public async Task<IActionResult> GetUpcomingMeetingsForMyBatch()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // Fetch all batchIds assigned to this user
            var batchIds = await _db.UserBatches
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.BatchId)
                .ToListAsync();

            if (!batchIds.Any())
                return BadRequest(new { message = "No batch assigned to your account" });

            var now = DateTime.UtcNow;
            var meetingsEntities = await _db.Meetings
                .Include(m => m.Batch)
                .Where(m => m.ScheduledAt >= now && batchIds.Contains(m.BatchId))
                .OrderBy(m => m.ScheduledAt)
                .ToListAsync();

            var meetings = meetingsEntities.Select(MapToDto).ToList();
            return Ok(meetings);
        }
    }
}
