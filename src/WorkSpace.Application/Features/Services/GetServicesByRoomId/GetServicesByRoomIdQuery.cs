using MediatR;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Wrappers;
using System.Collections.Generic;

namespace WorkSpace.Application.Features.Services.Queries.GetServicesByRoomId
{
    public class GetServicesByRoomIdQuery : IRequest<Response<IEnumerable<WorkSpaceServiceDto>>>
    {
        public int WorkSpaceRoomId { get; set; }
    }
}