using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Promotions.Queries.GetPromotionsByWorkspace
{
    public class GetPromotionsByWorkspaceQuery : IRequest<Response<IEnumerable<PromotionDto>>>
    {
        public int WorkSpaceId { get; set; }
    }
}