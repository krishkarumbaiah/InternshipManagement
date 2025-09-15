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
    public class QnAController : ControllerBase
    {
        private readonly AppDbContext _db;

        public QnAController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/qna
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var qna = await _db.Questions
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QnaDto
                {
                    Id = q.Id,
                    UserId = q.UserId,
                    UserName = q.User != null ? q.User.UserName ?? string.Empty : string.Empty,
                    Text = q.Text,
                    Answer = q.Answer,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            return Ok(qna);
        }

        // GET: api/qna/my
        [Authorize(Roles = "Intern")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyQuestions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            int parsedUserId = int.Parse(userId);

            var myQuestions = await _db.Questions
                .Where(q => q.UserId == parsedUserId)
                .Include(q => q.User)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QnaDto
                {
                    Id = q.Id,
                    UserId = q.UserId,
                    UserName = q.User != null ? q.User.UserName ?? string.Empty : string.Empty,
                    Text = q.Text,
                    Answer = q.Answer,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();

            return Ok(myQuestions);
        }

        // POST: api/qna/ask
        [Authorize(Roles = "Intern")]
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AskDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Question text is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var question = new Question
            {
                UserId = int.Parse(userId),
                Text = dto.Text.Trim()
            };

            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Question submitted successfully", id = question.Id });
        }

        // POST: api/qna/answer/{id}
        [Authorize(Roles = "Admin")]
        [HttpPost("answer/{id:int}")]
        public async Task<IActionResult> Answer(int id, [FromBody] AnswerDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Answer))
                return BadRequest("Answer text is required.");

            var question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound("Question not found");

            question.Answer = dto.Answer.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { message = "Answer submitted successfully" });
        }

        // PUT: api/qna/edit/{id} ( Intern can edit their own question)
        [Authorize(Roles = "Intern")]
        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> EditMyQuestion(int id, [FromBody] AskDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Question text is required.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound("Question not found");

            if (question.UserId != int.Parse(userId))
                return Forbid("You can only edit your own questions.");

            question.Text = dto.Text.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { message = "Your question was updated successfully" });
        }

        // PUT: api/qna/{id} (Admin can edit any question)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] AskDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Question text is required.");

            var question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound("Question not found");

            question.Text = dto.Text.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { message = "Question updated successfully (Admin)" });
        }

        // DELETE: api/qna/{id}
        // DELETE: api/qna/{id}
        [Authorize(Roles = "Admin,Intern")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var question = await _db.Questions.FindAsync(id);
            if (question == null) return NotFound("Question not found");

            //  If intern, only allow deleting own question
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            if (roles.Contains("Intern") && question.UserId != int.Parse(userId))
            {
                return Forbid("You can only delete your own questions.");
            }

            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Question deleted successfully" });
        }

    }
}
