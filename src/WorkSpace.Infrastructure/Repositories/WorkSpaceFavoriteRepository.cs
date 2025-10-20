using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{

    public class WorkSpaceFavoriteRepository : GenericRepositoryAsync<WorkSpaceFavorite>, IWorkSpaceFavoriteRepository
    {
        private readonly WorkSpaceContext _context;

        public WorkSpaceFavoriteRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }


        public async Task<bool> IsFavoriteExistsAsync(int userId, int workSpaceId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkSpaceFavorites
                .AnyAsync(f => f.UserId == userId && f.WorkspaceId == workSpaceId, cancellationToken);
        }
    }
}