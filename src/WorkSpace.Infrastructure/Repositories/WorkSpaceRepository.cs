using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class WorkSpaceRepository : GenericRepositoryAsync<WorkSpaces>, IWorkSpaceRepository
{
    public WorkSpaceRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
    }
}