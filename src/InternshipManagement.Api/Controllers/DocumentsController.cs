using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using System.IO;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentsController(AppDbContext context)
        {
            _context = context;
        }

        // Upload document using DTO
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // create unique file name to avoid collisions
            var ext = Path.GetExtension(dto.File.FileName);
            var savedFileName = $"{Guid.NewGuid()}{ext}";
            var savedFilePath = Path.Combine(uploadsFolder, savedFileName);

            using (var stream = new FileStream(savedFilePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // store relative path (so it is safe across environments)
            var relativePath = Path.Combine("uploads", savedFileName).Replace("\\", "/");

            var document = new Document
            {
                FileName = dto.File.FileName,
                FilePath = relativePath,
                ContentType = dto.File.ContentType ?? "application/octet-stream",
                UploadedAt = DateTimeOffset.UtcNow,
                UploadedBy = User?.Identity?.Name ?? "Unknown"
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File uploaded successfully", document.Id });
        }

        // List all documents
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var docs = await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return Ok(docs);
        }

        // Download document
        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            // combine wwwroot + relative path
            var filePhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart(Path.DirectorySeparatorChar, '/', '\\'));
            if (!System.IO.File.Exists(filePhysicalPath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePhysicalPath);
            var contentType = string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType;
            return File(fileBytes, contentType, document.FileName);
        }

        // DELETE: api/documents/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound(new { message = "Document not found" });

            // Delete the physical file if it exists
            if (System.IO.File.Exists(document.FilePath))
            {
                System.IO.File.Delete(document.FilePath);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Document deleted successfully" });
        }

        // Get documents uploaded by the logged-in user
        [HttpGet("my")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = User.Identity?.Name; // or better: get from claims
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var docs = await _context.Documents
                .Where(d => d.UploadedBy == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return Ok(docs);
        }

        // Delete my document
        [HttpDelete("my/{id}")]
        public async Task<IActionResult> DeleteMyDocument(int id)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null || doc.UploadedBy != userId)
                return NotFound(new { message = "Document not found or not yours" });

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Document deleted successfully" });
        }

    }
}
