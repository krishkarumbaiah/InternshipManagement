using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using System.Security.Claims;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // Intern submits feedback
        [HttpPost("submit")]
        [Authorize(Roles = "Intern,Student")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequestDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var feedback = new Feedback
            {
                UserId = userId,
                Comments = dto.Comments,
                Rating = dto.Rating
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Feedback submitted successfully!" });
        }

        // Admin sees all feedback
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFeedback()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.SubmittedAt)
                .Select(f => new FeedbackResponseDto
                {
                    Id = f.Id,
                    UserName = f.User.UserName,
                    Comments = f.Comments,
                    Rating = f.Rating,
                    SubmittedAt = f.SubmittedAt
                })
                .ToListAsync();

            return Ok(feedbacks);
        }

        // FeedbackController.cs (inside controller class)
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                    return NotFound(new { message = "Feedback not found" });

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Feedback deleted" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }

    }
}
