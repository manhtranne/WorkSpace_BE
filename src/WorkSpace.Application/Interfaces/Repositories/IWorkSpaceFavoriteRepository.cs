using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IWorkSpaceFavoriteRepository : IGenericRepositoryAsync<WorkSpaceFavorite>
    {

        Task<bool> IsFavoriteExistsAsync(int userId, int workSpaceId, CancellationToken cancellationToken = default);
    }
}