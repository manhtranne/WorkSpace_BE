using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class HostProfileAsyncProfileAsyncRepository : GenericRepositoryAsync<HostProfile>, IHostProfileAsyncRepository
{
    private readonly DbSet<HostProfile> _hosts;
    public HostProfileAsyncProfileAsyncRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _hosts = dbContext.Set<HostProfile>();
    }

    public Task<HostProfile?> GetHostProfileByUserId(int userId)
    {
        var hostProfile = _hosts.Include(h => h.User)
                            .FirstOrDefaultAsync(h => h.UserId == userId);
        if (hostProfile == null)
        {
            throw new KeyNotFoundException($"Host profile with id {userId} not found");
        }
        return hostProfile;
    }
}