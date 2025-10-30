using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Messages
{
    public class GetMessagesByChannelIdQuery : IRequest<ApiResponse<List<MessageDto>>>
    {
        public Guid ChannelId { get; }
        public int Take { get; }

        public GetMessagesByChannelIdQuery(Guid channelId, int take = 100)
        {
            ChannelId = channelId;
            Take = take;
        }
    }
}
