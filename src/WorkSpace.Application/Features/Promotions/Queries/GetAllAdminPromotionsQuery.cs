using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Promotions.Queries.GetAllAdminPromotions
{
    public class GetAllAdminPromotionsQuery : IRequest<Response<IReadOnlyList<PromotionDto>>>
    {
    }

    public class GetAllAdminPromotionsQueryHandler : IRequestHandler<GetAllAdminPromotionsQuery, Response<IReadOnlyList<PromotionDto>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IMapper _mapper;

        public GetAllAdminPromotionsQueryHandler(IPromotionRepository promotionRepository, IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _mapper = mapper;
        }

        public async Task<Response<IReadOnlyList<PromotionDto>>> Handle(GetAllAdminPromotionsQuery request, CancellationToken cancellationToken)
        {
            var promotions = await _promotionRepository.GetPromotionsByAdminAsync(cancellationToken);
            var result = _mapper.Map<IReadOnlyList<PromotionDto>>(promotions);
            return new Response<IReadOnlyList<PromotionDto>>(result);
        }
    }
}