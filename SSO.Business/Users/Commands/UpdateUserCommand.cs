﻿using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SSO.Business.Users.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        [JsonIgnore]
        public string? UserId { get; set; }

        [Required, MinLength(3)]
        public string FirstName { get; set; }

        [Required, MinLength(2)]
        public string LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 20 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Invalid characters in the username.")]
        public string Username { get; set; }

        [StringLength(100, MinimumLength = 7, ErrorMessage = "The password must be between {2} and {1} characters long.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{7,}$",
                   ErrorMessage = "The password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        public string? Password { get; set; }

        [Compare("Password")]
        public string? RepeatPassword { get; set; }

        [JsonIgnore]
        public string? Author { get; set; }
    }
}
