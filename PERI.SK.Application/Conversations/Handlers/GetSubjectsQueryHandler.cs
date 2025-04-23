using PERI.SK.Application.Conversations.Queries;
using PERI.SK.Domain.Interfaces;
using MediatR;

namespace PERI.SK.Application.Conversations.Handlers
{
    public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, List<string>>
    {
        readonly IChatService _chatService;

        public GetSubjectsQueryHandler(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<List<string>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
        {
            return await _chatService.GetSubjects();
        }
    }
}
