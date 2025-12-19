
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class PromotionRepository : GenericRepositoryAsync<Promotion>, IPromotionRepository
    {
        private readonly WorkSpaceContext _context;

        public PromotionRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive
                            && p.HostId == null
                            && p.StartDate <= now
                            && p.EndDate >= now
                            && (p.UsageLimit == 0 || p.UsedCount < p.UsageLimit))
                .OrderByDescending(p => p.EndDate)
                .ThenByDescending(p => p.DiscountValue)
                .ToListAsync(cancellationToken);
        }

        public async Task<Promotion> GetPromotionByCodeAsync(string code)
        {
            return await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IReadOnlyList<Promotion>> GetPromotionsByAdminAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.HostId == null)
                .OrderByDescending(p => p.CreateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Promotion>> GetPromotionsByHostIdAsync(int hostId, CancellationToken cancellationToken = default)
        {
            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.HostId == hostId)
                .OrderByDescending(p => p.CreateUtc)
                .ToListAsync(cancellationToken);
        }
        public async Task<IReadOnlyList<Promotion>> GetActivePromotionsByHostIdAsync(int hostId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.HostId == hostId
                            && p.IsActive
                            && p.StartDate <= now
                            && p.EndDate >= now
                            && (p.UsageLimit == 0 || p.UsedCount < p.UsageLimit)) 
                .OrderByDescending(p => p.EndDate)
                .ToListAsync(cancellationToken);
        }
    }
}