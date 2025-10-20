using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.Promotions.Queries
{
    public record GetActivePromotionsQuery(int Count = 5) : IRequest<IEnumerable<PromotionDto>>;

    public class GetActivePromotionsHandler(
        IPromotionRepository repository,
        IMapper mapper) : IRequestHandler<GetActivePromotionsQuery, IEnumerable<PromotionDto>>
    {
        public async Task<IEnumerable<PromotionDto>> Handle(GetActivePromotionsQuery request, CancellationToken cancellationToken)
        {
            var promotions = await repository.GetActivePromotionsAsync(request.Count, cancellationToken);
            var dtoList = mapper.Map<IEnumerable<PromotionDto>>(promotions);
            return dtoList;
        }
    }
}

