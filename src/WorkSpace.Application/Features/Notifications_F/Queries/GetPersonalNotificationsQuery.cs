using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Parameters;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Notifications.Queries
{
    public class GetPersonalNotificationsQuery : IRequest<PagedResponse<IEnumerable<NotificationDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetPersonalNotificationsQueryHandler : IRequestHandler<GetPersonalNotificationsQuery, PagedResponse<IEnumerable<NotificationDto>>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IMapper _mapper;

        public GetPersonalNotificationsQueryHandler(INotificationRepository notificationRepository, IAuthenticatedUserService authenticatedUserService, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _authenticatedUserService = authenticatedUserService;
            _mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<NotificationDto>>> Handle(GetPersonalNotificationsQuery request, CancellationToken cancellationToken)
        {
            var validFilter = new RequestParameter
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            var userId = int.Parse(_authenticatedUserService.UserId);

            var notifications = await _notificationRepository.GetRelevantOwnerNotificationsAsync(userId, validFilter.PageNumber, validFilter.PageSize);

            var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
            return new PagedResponse<IEnumerable<NotificationDto>>(notificationDtos, validFilter.PageNumber, validFilter.PageSize);
        }
    }
}