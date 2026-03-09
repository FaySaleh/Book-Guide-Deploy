using System.ComponentModel.DataAnnotations;

namespace BookGuide.API.DTOs.Auth
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }
}
