using Microsoft.AspNetCore.Identity;
using SSO.Domain.Authentication.Interfaces;
using SSO.Domain.Models;

namespace SSO.Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task Login(string username, string password, Application app)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user is null)
                throw new UnauthorizedAccessException("Username or password is invalid.");

            if (user.DateInactive != null)
                throw new UnauthorizedAccessException("Account is inactive");

            if (await _userManager.IsLockedOutAsync(user))
                throw new UnauthorizedAccessException("User is locked-out.");

            if (!(await _userManager.CheckPasswordAsync(user, password)))
            {
                if (app.MaxAccessFailedCount > 0 && app.MaxAccessFailedCount == user.AccessFailedCount)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.Now.AddMinutes(30);
                }

                await _userManager.AccessFailedAsync(user);

                user.LastFailedPasswordAttempt = DateTime.Now;

                await _userManager.UpdateAsync(user);

                throw new UnauthorizedAccessException("Username or password is invalid.");
            }

            user.LastSessionId = Guid.NewGuid().ToString();
            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);

            await _userManager.ResetAccessFailedCountAsync(user);
        }
    }
}
