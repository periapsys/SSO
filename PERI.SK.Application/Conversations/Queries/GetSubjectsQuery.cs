using MediatR;

namespace PERI.SK.Application.Conversations.Queries
{
    public class GetSubjectsQuery : IRequest<List<string>>
    {
    }
}
