
using MediatR;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Wrappers;
using System.Collections.Generic;

namespace WorkSpace.Application.Features.Services.Queries.GetAllServicesGrouped
{
    public class GetAllServicesGroupedQuery : IRequest<Response<List<WorkSpaceWithServicesDto>>>
    {
        public int OwnerUserId { get; set; }
    }
}