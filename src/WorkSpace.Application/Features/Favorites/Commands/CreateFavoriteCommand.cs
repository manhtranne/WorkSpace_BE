using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Interfaces.Repositories; 

namespace WorkSpace.Application.Features.Favorites.Commands
{

    public class CreateFavoriteCommand : IRequest<Response<int>>
    {
        public int WorkSpaceId { get; set; }
    }


    public class CreateFavoriteCommandHandler : IRequestHandler<CreateFavoriteCommand, Response<int>>
    {

        private readonly IWorkSpaceFavoriteRepository _favoriteRepository;
        private readonly IWorkSpaceRepository _workSpaceRepository; 
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IDateTimeService _dateTimeService;

        public CreateFavoriteCommandHandler(
            IWorkSpaceFavoriteRepository favoriteRepository, 
            IWorkSpaceRepository workSpaceRepository,       
            IAuthenticatedUserService authenticatedUserService,
            IDateTimeService dateTimeService)
        {
            _favoriteRepository = favoriteRepository;
            _workSpaceRepository = workSpaceRepository;
            _authenticatedUserService = authenticatedUserService;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<int>> Handle(CreateFavoriteCommand request, CancellationToken cancellationToken)
        {
        
            var userIdString = _authenticatedUserService.UserId;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                throw new ApiException("User is not authenticated.");
            }

   
            var workSpaceExists = await _workSpaceRepository.GetByIdAsync(request.WorkSpaceId, cancellationToken);

            if (workSpaceExists == null)
            {
                return new Response<int>($"WorkSpace with ID {request.WorkSpaceId} not found.");
            }


            var alreadyExists = await _favoriteRepository.IsFavoriteExistsAsync(userId, request.WorkSpaceId, cancellationToken);

            if (alreadyExists)
            {
                return new Response<int>(request.WorkSpaceId, "Đã thích không gian này từ trước.");
            }

  
            var newFavorite = new WorkSpaceFavorite
            {
                UserId = userId,
                WorkspaceId = request.WorkSpaceId,
                CreatedById = userId,
                CreateUtc = _dateTimeService.NowUtc
            };

            await _favoriteRepository.AddAsync(newFavorite, cancellationToken);

            return new Response<int>(request.WorkSpaceId, "Đã thêm vào danh sách yêu thích.");
        }
    }
}