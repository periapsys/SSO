using PERI.SK.Application.Conversations.Queries;
using PERI.SK.Domain.Interfaces;
using MediatR;

namespace PERI.SK.Application.Conversations.Handlers
{
    public class GetResponseQueryHandler : IRequestHandler<GetResponseQuery, string>
    {
        readonly IChatService _chatService;

        public GetResponseQueryHandler(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<string> Handle(GetResponseQuery request, CancellationToken cancellationToken)
        {
            return await _chatService.Converse(request.Query, request.Requestor);
        }
    }
}
