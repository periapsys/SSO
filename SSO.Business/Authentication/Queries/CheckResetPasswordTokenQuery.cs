using MediatR;

namespace SSO.Business.Authentication.Queries
{
    public class CheckResetPasswordTokenQuery : IRequest<Unit>
    {
        public Guid Token { get; set; }
    }
}
