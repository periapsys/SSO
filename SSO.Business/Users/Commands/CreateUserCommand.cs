using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SSO.Business.Users.Commands
{
    public class CreateUserCommand : IRequest<UserDto>
    {
        [JsonIgnore]
        public Guid RealmId { get; set; }

        [Required, MinLength(3)]
        public string FirstName { get; set; }

        [Required, MinLength(2)]
        public string LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 20 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "The password must be between {2} and {1} characters long.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{7,}$",
                   ErrorMessage = "The password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string RepeatPassword { get; set; }

        [JsonIgnore]
        public string? Author { get; set; }
    }
}
