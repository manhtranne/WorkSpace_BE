using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class HostProfileAsyncProfileAsyncRepository : GenericRepositoryAsync<HostProfile>, IHostProfileAsyncRepository
    {
<<<<<<< Updated upstream
        _hosts = dbContext.Set<HostProfile>();
    }

    public Task<HostProfile?> GetHostProfileByUserId(int userId, CancellationToken cancellationToken)
    {
        var hostProfile = _hosts.AsNoTracking()
                            .FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
        return hostProfile;
=======
        private readonly DbSet<HostProfile> _hosts;
        public HostProfileAsyncProfileAsyncRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _hosts = dbContext.Set<HostProfile>();
        }

        public Task<HostProfile?> GetHostProfileByUserId(int userId, CancellationToken cancellationToken)
        {
            var hostProfile = _hosts.AsNoTracking()
                                    .FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
            return hostProfile;
        }

        public async Task<IEnumerable<HostProfile>> GetAllHostProfilesAsync(
            int pageNumber,
            int pageSize,
            bool? isVerified,
            string? companyName,
            string? city, // Tham số 'city' bây giờ sẽ được dùng để tìm theo 'District'
            CancellationToken cancellationToken)
        {
            var query = _hosts
                .Include(h => h.User)
                .Include(h => h.Workspaces)
                    .ThenInclude(w => w.Address) // Cần Include Address để query
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (isVerified.HasValue)
            {
                query = query.Where(h => h.IsVerified == isVerified.Value);
            }

            if (!string.IsNullOrEmpty(companyName))
            {
                query = query.Where(h => h.CompanyName!.Contains(companyName));
            }

            // --- SỬA LỖI TẠI ĐÂY ---
            // Thay thế query theo City bằng District
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(h => h.Workspaces.Any(w => w.Address!.District == city));
            }

            return await query
                .OrderByDescending(h => h.IsVerified)
                .ThenByDescending(h => h.CreateUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasActiveWorkspacesAsync(int hostId, CancellationToken cancellationToken)
        {
            return await _hosts
                .Where(h => h.Id == hostId)
                .SelectMany(h => h.Workspaces)
                .AnyAsync(w => w.IsActive, cancellationToken);
        }
>>>>>>> Stashed changes
    }
}