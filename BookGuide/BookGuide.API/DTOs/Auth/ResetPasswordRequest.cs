using System.ComponentModel.DataAnnotations;

namespace BookGuide.API.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = "";

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = "";
    }
}
