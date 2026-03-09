using BookGuide.API.Data;
using BookGuide.API.DTOs;
using BookGuide.API.DTOs.Auth;
using BookGuide.API.Models;
using BookGuide.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;

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
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("FullName, Email, Password are required.");
            }

            var email = request.Email.Trim().ToLower();

            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == email);
            if (exists)
                return Conflict("Email already exists.");

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = HashPassword(request.Password),
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.CreatedAt
            });
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

            var passwordHash = HashPassword(request.Password);
            if (user.PasswordHash != passwordHash)
                return Unauthorized("Invalid email or password.");

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email
            });
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
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

            record.User.PasswordHash = HashPassword(req.NewPassword);

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
   
