using BookGuide.API.Data;
using BookGuide.API.DTOs;
using BookGuide.API.DTOs.Auth;
using BookGuide.API.Models;
using BookGuide.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(BookGuideDbContext db, IEmailSender email, IConfiguration config, ILogger<AuthController> logger)
        {
            _db = db;
            _email = email;
            _config = config;
            _logger = logger;


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
            try
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
            catch (Exception ex)
            {
                Console.WriteLine("LOGIN ERROR:");
                Console.WriteLine(ex.ToString());

                return StatusCode(500, new
                {
                    message = "Login failed",
                    error = ex.Message
                });
            }
        }




        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest dto)
        {
            var email = dto.Email?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(email))
                return Ok(new { message = "If the email exists, a reset link will be sent." });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
                return Ok(new { message = "If the email exists, a reset link will be sent." });

            var token = Guid.NewGuid().ToString("N");
            var tokenHash = Sha256(token);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiryAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            _db.PasswordResetTokens.Add(resetToken);
            await _db.SaveChangesAsync();

            var frontendBaseUrl = _config["App:FrontendBaseUrl"]?.Trim().TrimEnd('/');



            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                return StatusCode(500, new
                {
                    message = "FrontendBaseUrl is not configured."
                });
            }

            var resetUrl = $"{frontendBaseUrl}/reset-password?token={token}";

            var html = EmailTemplates.ResetPasswordHtml(
                "BookGuide",
                "https://i.ibb.co/TMzSkvpW/Logo.png",
                user.FullName,
                resetUrl
            );
            try
            {
                await _email.SendAsync(user.Email, "Reset your password", html);

                return Ok(new
                {
                    message = "Reset link sent successfully.",
                    resetUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Forgot password email failed for {Email}", user.Email);

                return StatusCode(500, new
                {
                    message = "Email sending failed.",
                    error = ex.Message
                });
            }

        }

       

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = req.Token.Trim();
            if (token.Length < 20)
                return BadRequest("Invalid token.");

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