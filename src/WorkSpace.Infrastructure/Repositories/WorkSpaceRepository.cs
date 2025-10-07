using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class WorkSpaceRepository : GenericRepositoryAsync<Domain.Entities.WorkSpace>, IWorkSpaceRepository
{
    private readonly DbSet<WorkSpace.Domain.Entities.WorkSpace> _workSpaces;
    public WorkSpaceRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _workSpaces = dbContext.Set<WorkSpace.Domain.Entities.WorkSpace>();
    }
}