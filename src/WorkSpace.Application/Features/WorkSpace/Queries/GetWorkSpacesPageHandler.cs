using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries;

public record GetWorkSpacesPagedQuery(
    WorkSpaceFilter filter,
    int pageNumber = 1,
    int pageSize = 10
) : IRequest<PagedResponse<IEnumerable<WorkSpaceListItemDto>>>;

public class GetWorkSpacesPageHandler(
    IWorkSpaceRepository repository,
    IMapper mapper) : IRequestHandler<GetWorkSpacesPagedQuery, PagedResponse<IEnumerable<WorkSpaceListItemDto>>>
{
    public async Task<PagedResponse<IEnumerable<WorkSpaceListItemDto>>> Handle(GetWorkSpacesPagedQuery request,
        CancellationToken cancellationToken)
    {
        var (item,count) = await repository.GetPagedAsync(request.filter,request.pageNumber, request.pageSize, cancellationToken);
        
        var dtoList = mapper.Map<IEnumerable<WorkSpaceListItemDto>>(item);
        var pagedResponse = new PagedResponse<IEnumerable<WorkSpaceListItemDto>>(dtoList, request.pageNumber, request.pageSize)
        {
            Message = $"Tổng số bản ghi : {count}"
        };
        return pagedResponse;
    }
}