using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Exceptions; 
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Promotions.Queries.GetPromotionsByWorkspace
{
    public class GetPromotionsByWorkspaceQueryHandler : IRequestHandler<GetPromotionsByWorkspaceQuery, Response<IEnumerable<PromotionDto>>>
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IWorkSpaceRepository _workSpaceRepository;
        private readonly IMapper _mapper;

        public GetPromotionsByWorkspaceQueryHandler(
            IPromotionRepository promotionRepository,
            IWorkSpaceRepository workSpaceRepository,
            IMapper mapper)
        {
            _promotionRepository = promotionRepository;
            _workSpaceRepository = workSpaceRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<PromotionDto>>> Handle(GetPromotionsByWorkspaceQuery request, CancellationToken cancellationToken)
        {
            var workspace = await _workSpaceRepository.GetByIdAsync(request.WorkSpaceId);

            if (workspace == null)
            {
                throw new ApiException($"Workspace with ID {request.WorkSpaceId} not found.");
            }

   
            var promotions = await _promotionRepository.GetActivePromotionsByHostIdAsync(workspace.HostId, cancellationToken);

            var promotionDtos = _mapper.Map<IEnumerable<PromotionDto>>(promotions);

            return new Response<IEnumerable<PromotionDto>>(promotionDtos);
        }
    }
}