using PERI.SK.Application.Conversations.Queries;
using PERI.SK.Domain.Interfaces;
using MediatR;

namespace PERI.SK.Application.Conversations.Handlers
{
    public class GetPromptQueryHandler : IRequestHandler<GetPromptQuery, string>
    {
        readonly IChatService _chatService;

        public GetPromptQueryHandler(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<string> Handle(GetPromptQuery request, CancellationToken cancellationToken)
        {
            return await _chatService.GetPrompt(request.Key);
        }
    }
}
