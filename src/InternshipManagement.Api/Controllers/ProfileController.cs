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

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
                    ProfilePhoto = string.IsNullOrEmpty(u.ProfilePhotoPath)
                        ? null
                        : $"{Request.Scheme}://{Request.Host}{u.ProfilePhotoPath}" // return absolute URL
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
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

                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
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

                if (!string.IsNullOrWhiteSpace(user.ProfilePhotoPath))
                {
                    try
                    {
                        var old = user.ProfilePhotoPath.TrimStart('/');
                        var oldFull = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", old.Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldFull))
                        {
                            System.IO.File.Delete(oldFull);
                        }
                    }
                    catch { /* ignore errors */ }
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
                ProfilePhoto = string.IsNullOrEmpty(user.ProfilePhotoPath)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}{user.ProfilePhotoPath}" // absolute URL
            });
        }
    }
}
    