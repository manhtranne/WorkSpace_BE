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

        public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(int count = 5, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive 
                            && p.StartDate <= now 
                            && p.EndDate >= now
                            && (p.UsageLimit == 0 || p.UsedCount < p.UsageLimit))
                .OrderByDescending(p => p.EndDate)
                .ThenByDescending(p => p.DiscountValue)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}

