using WorkSpace.Application.DTOs.Search;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Interfaces.Services;

public interface IWorkSpaceService
{
    Task AddUserToWorkSpaceAsync(string workSpaceId, string userId);

    Task<Response<List<SearchWorkspaceDto>>> GetFeaturedAsync(int take = 4);
}
