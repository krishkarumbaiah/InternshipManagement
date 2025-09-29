using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // must be logged in
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        // ✅ Helper: build absolute photo URL
        private string? GetPhotoUrl(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            var physicalPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(physicalPath))
            {
                // fallback default avatar
                return $"{Request.Scheme}://{Request.Host}/assets/default-avatar.png";
            }

            return $"{Request.Scheme}://{Request.Host}{relativePath}";
        }

        // GET: api/profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FullName,
                    u.ProfilePhotoPath
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                ProfilePhoto = GetPhotoUrl(user.ProfilePhotoPath)
            });
        }

        // PUT: api/profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound(new { message = "User not found" });

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new { message = "Full name is required" });

            string? newRelativePath = null;

            if (dto.ProfilePhoto != null && dto.ProfilePhoto.Length > 0)
            {
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                var contentType = dto.ProfilePhoto.ContentType?.ToLowerInvariant() ?? "";
                if (!allowedContentTypes.Contains(contentType))
                    return BadRequest(new { message = "Only JPG, PNG or WEBP images are allowed" });

                const long maxBytes = 2 * 1024 * 1024; // 2MB
                if (dto.ProfilePhoto.Length > maxBytes)
                    return BadRequest(new { message = "Profile photo max size is 2 MB" });

                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsRoot)) Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(dto.ProfilePhoto.FileName);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    ext = contentType switch
                    {
                        "image/png" => ".png",
                        "image/webp" => ".webp",
                        _ => ".jpg"
                    };
                }

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePhoto.CopyToAsync(stream);
                }

                newRelativePath = $"/uploads/profiles/{fileName}";

                // delete old photo
                if (!string.IsNullOrWhiteSpace(user.ProfilePhotoPath))
                {
                    try
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, user.ProfilePhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    catch { /* ignore */ }
                }
            }

            user.FullName = dto.FullName.Trim();
            if (newRelativePath != null)
                user.ProfilePhotoPath = newRelativePath;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return StatusCode(500, new
                {
                    message = "Failed to update profile",
                    errors = updateResult.Errors.Select(e => e.Description).ToArray()
                });
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FullName,
                ProfilePhoto = GetPhotoUrl(user.ProfilePhotoPath)
            });
        }
    }
}
