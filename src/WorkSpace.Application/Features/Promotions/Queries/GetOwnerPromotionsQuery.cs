
using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Promotions.Queries.GetOwnerPromotions
{
    public class GetOwnerPromotionsQuery : IRequest<Response<IReadOnlyList<PromotionDto>>>
    {
        public int UserId { get; set; }
    }

    public class GetOwnerPromotionsQueryHandler : IRequestHandler<GetOwnerPromotionsQuery, Response<IReadOnlyList<PromotionDto>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IHostProfileAsyncRepository _hostProfileRepository;
        private readonly IMapper _mapper;

        public GetOwnerPromotionsQueryHandler(
            IPromotionRepository promotionRepository,
            IHostProfileAsyncRepository hostProfileRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _hostProfileRepository = hostProfileRepository;
            _mapper = mapper;
        }

        public async Task<Response<IReadOnlyList<PromotionDto>>> Handle(GetOwnerPromotionsQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostProfileRepository.GetHostProfileByUserId(request.UserId, cancellationToken);

            if (hostProfile == null)
            {
                throw new ApiException("Host profile not found for this user.");
            }

            var promotions = await _promotionRepository.GetPromotionsByHostIdAsync(hostProfile.Id, cancellationToken);

            var result = _mapper.Map<IReadOnlyList<PromotionDto>>(promotions);
            return new Response<IReadOnlyList<PromotionDto>>(result);
        }
    }
}