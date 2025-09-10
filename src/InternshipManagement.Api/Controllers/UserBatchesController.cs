using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserBatchesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserBatchesController(AppDbContext db) => _db = db;

        // GET: api/userbatches/byuser/5
        [HttpGet("byuser/{userId:int}")]
        public async Task<IActionResult> ByUser(int userId)
        {
            var items = await _db.UserBatches
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Batch)
                .ToListAsync();
            return Ok(items);
        }

        // GET: api/userbatches/bybatch/5
        [HttpGet("bybatch/{batchId:int}")]
        public async Task<IActionResult> ByBatch(int batchId)
        {
            var items = await _db.UserBatches
                .Where(ub => ub.BatchId == batchId)
                .Include(ub => ub.User)
                .ToListAsync();
            return Ok(items);
        }

        // POST: api/userbatches/assign
        // Admin only
        [Authorize(Roles = "Admin")]
        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // check user exists
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound($"User {dto.UserId} not found");

            // check batch exists
            var batch = await _db.Batches.FindAsync(dto.BatchId);
            if (batch == null) return NotFound($"Batch {dto.BatchId} not found");

            // prevent duplicate
            var exists = await _db.UserBatches.AnyAsync(ub => ub.UserId == dto.UserId && ub.BatchId == dto.BatchId);
            if (exists) return Conflict("User already assigned to this batch.");

            var ubEntry = new UserBatch
            {
                UserId = dto.UserId,
                BatchId = dto.BatchId
            };
            _db.UserBatches.Add(ubEntry);
            await _db.SaveChangesAsync();
            return Ok(ubEntry);
        }

        // DELETE: api/userbatches/unassign/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("unassign/{id:int}")]
        public async Task<IActionResult> Unassign(int id)
        {
            var ub = await _db.UserBatches.FindAsync(id);
            if (ub == null) return NotFound();
            _db.UserBatches.Remove(ub);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public record AssignDto([Required] int UserId, [Required] int BatchId);
}
