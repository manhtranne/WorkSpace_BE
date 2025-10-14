
using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
    public record GetWorkSpaceRoomsPagedQuery(
        WorkSpaceFilter filter,
        int pageNumber = 1,
        int pageSize = 10
    ) : IRequest<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>>;

    public class GetWorkSpaceRoomsPageHandler(
        IWorkSpaceRepository repository,
        IMapper mapper) : IRequestHandler<GetWorkSpaceRoomsPagedQuery, PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>>
    {
        public async Task<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>> Handle(GetWorkSpaceRoomsPagedQuery request,
            CancellationToken cancellationToken)
        {
            var (rooms, count) = await repository.GetRoomsPagedAsync(request.filter, request.pageNumber, request.pageSize, cancellationToken);

            var dtoList = mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(rooms);
            var pagedResponse = new PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>(dtoList, request.pageNumber, request.pageSize)
            {
                Message = $"Total records: {count}"
            };
            return pagedResponse;
        }
    }
}