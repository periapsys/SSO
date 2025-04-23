using MediatR;

namespace PERI.SK.Application.Conversations.Queries
{
    public class GetResponseQuery : IRequest<string>
    {
        public required string Query { get; set; }
        public required string Requestor { get; set; }
    }
}
