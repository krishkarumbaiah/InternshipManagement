using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using InternshipManagement.Api.Data;
using System.Security.Claims;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaveController(AppDbContext context)
        {
            _context = context;
        }

        // Intern/Student applies for leave
        [HttpPost("apply")]
        [Authorize(Roles = "Student,Intern")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveRequestDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid leave request");

                
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var leave = new Leave
                {
                    UserId = userId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Reason = dto.Reason,
                    Status = "Pending"
                };

                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Leave applied successfully", LeaveId = leave.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }

        // Admin sees all leave requests
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLeaves()
        {
            var leaves = await _context.Leaves
                .Include(l => l.User) 
                .Select(l => new LeaveResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User.UserName, 
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Reason = l.Reason,
                    Status = l.Status
                })
                .ToListAsync();

            return Ok(leaves);
        }

        // Admin approves/rejects leave
        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLeaveStatus(int id, [FromQuery] string status)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null)
                return NotFound("Leave request not found");

            if (status != "Approved" && status != "Rejected")
                return BadRequest("Invalid status");

            leave.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Leave {status}" });
        }

        // Intern views own leaves
        [HttpGet("my-leaves")]
        [Authorize(Roles = "Intern,Student")]
        public async Task<IActionResult> GetMyLeaves()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var leaves = await _context.Leaves
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.StartDate)
                .Select(l => new LeaveResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Reason = l.Reason,
                    Status = l.Status
                })
                .ToListAsync();

            return Ok(leaves);
        }

        // Admin deletes any leave
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null)
                return NotFound(new { message = "Leave request not found" });

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave request deleted successfully" });
        }

        // Intern cancels their own pending leave
        [HttpDelete("cancel/{id}")]
        [Authorize(Roles = "Intern,Student")]
        public async Task<IActionResult> CancelLeave(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null || leave.UserId != userId)
            {
                return NotFound(new { message = "Leave not found or not yours" });
            }

            if (leave.Status != "Pending")
            {
                return BadRequest(new { message = "Only pending leaves can be cancelled" });
            }

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave request cancelled successfully" });
        }
    }
}
