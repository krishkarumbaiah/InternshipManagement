using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AttendanceController(AppDbContext db)
        {
            _db = db;
        }

        // POST: api/attendance/mark
        [Authorize(Roles = "Admin")]
        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Prevent duplicate attendance on same day
            var exists = await _db.Attendance
                .AnyAsync(a => a.UserId == dto.UserId && a.Date.Date == dto.Date.Date);

            if (exists) return Conflict("Attendance already marked for this user on this date.");

            var attendance = new Attendance
            {
                UserId = dto.UserId,
                BatchId = dto.BatchId,
                Date = dto.Date.Date,
                IsPresent = dto.IsPresent
            };

            _db.Attendance.Add(attendance);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Attendance marked successfully" });
        }

        // GET: api/attendance/byuser/5  (Admin only – explicit userId)
        [Authorize(Roles = "Admin")]
        [HttpGet("byuser/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var list = await _db.Attendance
                .Where(a => a.UserId == userId)
                .Include(a => a.Batch)
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.UserName,
                    BatchId = a.BatchId,
                    BatchName = a.Batch.Name,
                    Date = a.Date,
                    IsPresent = a.IsPresent
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/attendance/my (Interns see their own attendance)
        [Authorize(Roles = "Intern")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not found");

            int parsedUserId = int.Parse(userId);

            var list = await _db.Attendance
                .Where(a => a.UserId == parsedUserId)
                .Include(a => a.Batch)
                .Include(a => a.User)
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.UserName,
                    BatchId = a.BatchId,
                    BatchName = a.Batch.Name,
                    Date = a.Date,
                    IsPresent = a.IsPresent
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/attendance/bybatch/3
        [Authorize(Roles = "Admin")]
        [HttpGet("bybatch/{batchId:int}")]
        public async Task<IActionResult> GetByBatch(int batchId)
        {
            var list = await _db.Attendance
                .Where(a => a.BatchId == batchId)
                .Include(a => a.User)
                .Include(a => a.Batch)
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.UserName,
                    BatchId = a.BatchId,
                    BatchName = a.Batch.Name,
                    Date = a.Date,
                    IsPresent = a.IsPresent
                })
                .ToListAsync();

            return Ok(list);
        }
        
        // DELETE: api/attendance/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var record = await _db.Attendance.FindAsync(id);
            if (record == null) return NotFound("Attendance record not found");

            _db.Attendance.Remove(record);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Attendance record deleted successfully" });
        }

    }
}
