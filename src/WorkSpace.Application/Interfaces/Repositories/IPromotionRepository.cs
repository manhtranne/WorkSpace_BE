using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IPromotionRepository : IGenericRepositoryAsync<Promotion>
    {
        Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default);
        Task<Promotion> GetPromotionByCodeAsync(string code);
        Task<IReadOnlyList<Promotion>> GetPromotionsByAdminAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Promotion>> GetPromotionsByHostIdAsync(int hostId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Promotion>> GetActivePromotionsByHostIdAsync(int hostId, CancellationToken cancellationToken = default);
    }
}

