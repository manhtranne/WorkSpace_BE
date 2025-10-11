using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IWorkSpaceRepository : IGenericRepositoryAsync<Domain.Entities.WorkSpace>
{
    Task<WorkSpace.Domain.Entities.WorkSpace?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<WorkSpace.Domain.Entities.WorkSpace>,
        int TotalCount)> GetPagedAsync(
        WorkSpaceFilter filter, 
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
        );
    
    Task<bool> ExistsTitleForHostAsync(int hostId, string title, CancellationToken cancellationToken = default);
}