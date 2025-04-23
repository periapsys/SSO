using MediatR;

namespace PERI.SK.Application.Conversations.Queries
{
    public class GetPromptQuery : IRequest<string>
    {
        public required string Key { get; set; }
    }
}
