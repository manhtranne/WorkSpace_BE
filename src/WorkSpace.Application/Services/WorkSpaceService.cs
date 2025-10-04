using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Application.Services;

public class WorkSpaceService : IWorkSpaceService
{
    private readonly IWorkSpaceRepository _workSpaceRepository;
    public WorkSpaceService(IWorkSpaceRepository workSpaceRepository)
    {
        _workSpaceRepository = workSpaceRepository;
    }
    public Task AddUserToWorkSpaceAsync(string workSpaceId, string userId)
    {
        throw new NotImplementedException();
    }
}