using InternshipManagement.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReportsController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/reports/overview
        [Authorize(Roles = "Admin")]
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalBatches = await _db.Batches.CountAsync();
            var totalQuestions = await _db.Questions.CountAsync();
            var unanswered = await _db.Questions.CountAsync(q => q.Answer == null);

            var attendanceRecords = await _db.Attendance.CountAsync();
            var presentCount = await _db.Attendance.CountAsync(a => a.IsPresent);
            double attendanceRate = attendanceRecords > 0 ? (presentCount * 100.0 / attendanceRecords) : 0;

            return Ok(new
            {
                users = new { total = totalUsers },
                batches = totalBatches,
                qnA = new { total = totalQuestions, unanswered },
                attendance = new { totalRecords = attendanceRecords, presentRate = attendanceRate }
            });
        }

        // GET: api/reports/batch-attendance/{batchId}
        [Authorize(Roles = "Admin")]
        [HttpGet("batch-attendance/{batchId:int}")]
        public async Task<IActionResult> GetBatchAttendance(int batchId)
        {
            var total = await _db.Attendance.CountAsync(a => a.BatchId == batchId);
            var present = await _db.Attendance.CountAsync(a => a.BatchId == batchId && a.IsPresent);

            double percent = total > 0 ? (present * 100.0 / total) : 0;

            return Ok(new
            {
                batchId,
                totalRecords = total,
                presentRate = percent
            });
        }

        // ===============================
        // 🙋 Intern Reports
        // ===============================

        // GET: api/reports/myoverview
        [Authorize(Roles = "Intern")]
        [HttpGet("myoverview")]
        public async Task<IActionResult> MyOverview()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            // Attendance
            var totalAttendance = await _db.Attendance.CountAsync(a => a.UserId == userId);
            var presentDays = await _db.Attendance.CountAsync(a => a.UserId == userId && a.IsPresent);
            double attendancePercent = totalAttendance > 0 ? (presentDays * 100.0 / totalAttendance) : 0;

            // QnA
            var totalQuestions = await _db.Questions.CountAsync(q => q.UserId == userId);
            var answeredQuestions = await _db.Questions.CountAsync(q => q.UserId == userId && q.Answer != null);
            var unansweredQuestions = totalQuestions - answeredQuestions;

            return Ok(new
            {
                attendance = new
                {
                    totalDays = totalAttendance,
                    presentDays,
                    presentRate = attendancePercent
                },
                questions = new
                {
                    total = totalQuestions,
                    answered = answeredQuestions,
                    unanswered = unansweredQuestions
                }
            });
        }
    }
}
