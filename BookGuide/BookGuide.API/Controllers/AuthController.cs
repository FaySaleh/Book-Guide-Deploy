using BookGuide.API.Data;
using BookGuide.API.DTOs;
using BookGuide.API.DTOs.Auth;
using BookGuide.API.Models;
using BookGuide.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BookGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BookGuideDbContext _db;
        private readonly IEmailSender _email;
        private readonly IConfiguration _config;

        public AuthController(BookGuideDbContext db, IEmailSender email, IConfiguration config)
        {
            _db = db;
            _email = email;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request.");

                var fullName = dto.FullName?.Trim();
                var email = dto.Email?.Trim().ToLower();
                var password = dto.Password?.Trim();

                if (string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    return BadRequest("Full name, email, and password are required.");
                }

                var exists = await _db.Users.AnyAsync(x => x.Email.ToLower() == email);
                if (exists)
                    return BadRequest("Email already exists");

                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    user.Id,
                    user.FullName,
                    user.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("REGISTER ERROR:");
                Console.WriteLine(ex);

                return StatusCode(500, ex.ToString());
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            var email = request.Email.Trim().ToLower();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest dto)
        {
            var email = dto.Email?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(email))
                return Ok();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
                return Ok();

            var token = Guid.NewGuid().ToString("N");
            var tokenHash = Sha256(token);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiryAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            _db.PasswordResetTokens.Add(resetToken);
            await _db.SaveChangesAsync();

            var resetUrl = $"http://localhost:4200/reset-password?token={token}";

            var html = EmailTemplates.ResetPasswordHtml(
                "BookGuide",
                "https://i.ibb.co/TMzSkvpW/Logo.png",
                user.FullName,
                resetUrl
            );

            await _email.SendAsync(
                user.Email,
                "Reset your BookGuide password",
                html
            );

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var token = req.Token.Trim();
            if (token.Length < 20) return BadRequest("Invalid token.");

            var tokenHash = Sha256(token);
            var now = DateTime.UtcNow;

            var record = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    !t.IsUsed &&
                    t.ExpiryAt > now);

            if (record == null)
                return BadRequest("Token is invalid or expired.");

            record.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

            record.IsUsed = true;
            record.UsedAt = now;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}