using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
    public class BatchesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BatchesController(AppDbContext db) => _db = db;

        // GET: api/batches
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Batches
                .Select(b => new BatchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/batches/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var batch = await _db.Batches
                .Where(b => b.Id == id)
                .Select(b => new BatchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate
                })
                .FirstOrDefaultAsync();

            if (batch == null) return NotFound();
            return Ok(batch);
        }

        // POST: api/batches
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBatchDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.StartDate >= dto.EndDate) return BadRequest("StartDate must be before EndDate.");

            var batch = new Batch
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _db.Batches.Add(batch);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = batch.Id }, new BatchDto
            {
                Id = batch.Id,
                Name = batch.Name,
                StartDate = batch.StartDate,
                EndDate = batch.EndDate
            });
        }

        // PUT: api/batches/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBatchDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var batch = await _db.Batches.FindAsync(id);
            if (batch == null) return NotFound();

            if (dto.StartDate >= dto.EndDate) return BadRequest("StartDate must be before EndDate.");

            batch.Name = dto.Name;
            batch.StartDate = dto.StartDate;
            batch.EndDate = dto.EndDate;

            _db.Batches.Update(batch);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/batches/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var batch = await _db.Batches.FindAsync(id);
            if (batch == null) return NotFound();

            _db.Batches.Remove(batch);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

}
