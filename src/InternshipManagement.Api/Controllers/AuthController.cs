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

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAppEmailSender _emailSender;

        // 🔹 In-memory OTP store (email → otp + expiry)
        private static readonly Dictionary<string, (string Otp, DateTime Expiry)> _otpStore = new();

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            IAppEmailSender emailSender)
        {
            _userManager = userManager;
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

        // 🔹 2. Register with OTP verification
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // OTP Validation
            if (!_otpStore.ContainsKey(model.Email) ||
                _otpStore[model.Email].Otp != model.Otp ||
                _otpStore[model.Email].Expiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var role = string.IsNullOrWhiteSpace(model.Role) ? "Intern" : model.Role;
            await _userManager.AddToRoleAsync(user, role);

            // Remove OTP after success
            _otpStore.Remove(model.Email);

            return Ok(new { message = "User registered successfully" });
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
                roles = userRoles
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
