using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternshipManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using InternshipManagement.Api.Models.DTOs;
using InternshipManagement.Api.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _config;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAppEmailSender _emailSender;

        // in-memory OTP store (small projects only)
        private static readonly Dictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            IAppEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // ================= OTP Flow =================

        // 🔹 1. Send OTP
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required" });

            var otp = new Random().Next(100000, 999999).ToString();

            // Save OTP with 5-minute expiry
            _otpStore[email] = (otp, DateTime.UtcNow.AddMinutes(5));

            await _emailSender.SendEmailAsync(email, "Registration OTP", $"Your OTP is: {otp}");

            return Ok(new { message = "OTP sent successfully" });
        }

        // 🔹 2. Register with OTP verification + optional profile photo
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // OTP Validation
            if (!_otpStore.ContainsKey(model.Email) ||
                _otpStore[model.Email].Otp != model.Otp ||
                _otpStore[model.Email].Expiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            // Pre-checks
            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return BadRequest(new { message = $"Username '{model.UserName}' is already taken." });

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { message = $"Email '{model.Email}' is already registered." });

            // Handle profile photo upload if present
            string? savedRelativePath = null;
            if (model.ProfilePhoto != null && model.ProfilePhoto.Length > 0)
            {
                // validate content type
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                var contentType = model.ProfilePhoto.ContentType?.ToLowerInvariant() ?? string.Empty;
                if (!allowedContentTypes.Contains(contentType))
                {
                    return BadRequest(new { message = "Only JPG, PNG or WEBP images are allowed for profile photo." });
                }

                const long maxBytes = 2 * 1024 * 1024; // 2 MB
                if (model.ProfilePhoto.Length > maxBytes)
                {
                    return BadRequest(new { message = "Profile photo max size is 2 MB." });
                }

                // Ensure directory exists
                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsRoot))
                    Directory.CreateDirectory(uploadsRoot);

                var ext = Path.GetExtension(model.ProfilePhoto.FileName);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    // fallback to extension by content type
                    ext = contentType switch
                    {
                        "image/png" => ".png",
                        "image/webp" => ".webp",
                        _ => ".jpg"
                    };
                }

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePhoto.CopyToAsync(stream);
                    }

                    // saved relative path for storage and retrieval by client
                    savedRelativePath = $"/uploads/profiles/{fileName}";
                }
                catch (Exception ex)
                {
                    // log ex in real app (ILogger)
                    return StatusCode(500, new { message = "Failed to save profile photo", error = ex.Message });
                }
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                ProfilePhotoPath = savedRelativePath
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { errors });
            }

            // Assign Intern role server-side (ignore client role)
            var internRoleName = "Intern";
            if (!await _roleManager.RoleExistsAsync(internRoleName))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole<int>(internRoleName));
                if (!createRoleResult.Succeeded)
                {
                    // optionally rollback the newly created user in critical systems
                    return StatusCode(500, new { message = "Failed to create default role" });
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, internRoleName);
            if (!addRoleResult.Succeeded)
            {
                // optionally rollback the newly created user in critical systems
                return StatusCode(500, new { message = "User created but failed to assign default role" });
            }

            // Remove OTP after successful registration
            _otpStore.Remove(model.Email);

            // Return created user info (avoid sensitive data)
            return Ok(new
            {
                message = "User registered successfully",
                userId = user.Id,
                profilePhoto = user.ProfilePhotoPath
            });
        }

        // ================= Login =================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName) ?? await _userManager.FindByEmailAsync(model.UserName);
            if (user == null) return Unauthorized("Invalid credentials");

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid) return Unauthorized("Invalid credentials");

            var userRoles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };
            claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            var jwtSettings = _config.GetSection("JwtSettings");
            var key = jwtSettings.GetValue<string>("Key");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");
            var expireMinutes = jwtSettings.GetValue<int>("ExpireMinutes");

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expires = tokenDescriptor.Expires,
                username = user.UserName,
                roles = userRoles,
                userId = user.Id,
                profilePhoto = user.ProfilePhotoPath
            });
        }

        // ================= Forgot/Reset Password =================
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var email = model.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return BadRequest(new { message = "User not found" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var angularClientUrl = "http://localhost:4200/reset-password";
            var resetLink = $"{angularClientUrl}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(user.Email, "Password Reset",
                $"Click here to reset your password: <a href='{HtmlEncoder.Default.Encode(resetLink)}'>Reset</a>");

            return Ok(new { message = "Password reset link sent to email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    Console.WriteLine($"ResetPassword error: {err.Code} - {err.Description}");

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}
