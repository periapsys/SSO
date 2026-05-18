using MediatR;
using SSO.Business.Captchas;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SSO.Business.Authentication.Commands
{
    public class ResetPasswordCommand : IRequest<Unit>
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid Token { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "The password must be between {2} and {1} characters long.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{7,}$",
                   ErrorMessage = "The password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        public string RepeatPassword { get; set; }

        public CaptchaRequest Captcha { get; set; }

        [JsonIgnore]
        public Guid? RealmId { get; set; }
    }
}
