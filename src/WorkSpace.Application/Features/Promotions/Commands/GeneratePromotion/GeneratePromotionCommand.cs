using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Promotions.Commands.GeneratePromotion
{
    public class GeneratePromotionCommand : IRequest<Response<string>>
    {
        public GeneratePromotionDto Dto { get; set; }
        public int RequestUserId { get; set; }
        public bool IsOwnerCode { get; set; } 
    }

    public class GeneratePromotionCommandHandler : IRequestHandler<GeneratePromotionCommand, Response<string>>
    {
        private readonly IPromotionRepository _promotionRepo;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IDateTimeService _dateTimeService;
        private readonly IMapper _mapper;

        public GeneratePromotionCommandHandler(
            IPromotionRepository promotionRepo,
            IHostProfileAsyncRepository hostRepo,
            IDateTimeService dateTimeService,
            IMapper mapper)
        {
            _promotionRepo = promotionRepo;
            _hostRepo = hostRepo;
            _dateTimeService = dateTimeService;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(GeneratePromotionCommand request, CancellationToken cancellationToken)
        {
     
            var promotion = _mapper.Map<Promotion>(request.Dto);

     
            string randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            promotion.Code = $"SALE-{randomSuffix}";

      
            promotion.IsActive = false;
            promotion.CreateUtc = _dateTimeService.NowUtc;
            promotion.CreatedById = request.RequestUserId;

       
            if (request.IsOwnerCode)
            {
                var hostProfile = await _hostRepo.GetHostProfileByUserId(request.RequestUserId, cancellationToken);
                if (hostProfile == null)
                    throw new ApiException("Bạn không có hồ sơ Host để tạo mã.");

                promotion.HostId = hostProfile.Id;
            }
            else
            {
            
                promotion.HostId = null;
            }

            await _promotionRepo.AddAsync(promotion, cancellationToken);

            return new Response<string>(promotion.Code, $"Đã tạo mã {promotion.Code} (Chưa kích hoạt). ID: {promotion.Id}");
        }
    }
}