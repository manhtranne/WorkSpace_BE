using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Application.Features.Notifications.Queries
{
    public class GetPersonalNotificationsQuery : IRequest<IEnumerable<NotificationDto>>
    {
    }

    public class GetPersonalNotificationsQueryHandler : IRequestHandler<GetPersonalNotificationsQuery, IEnumerable<NotificationDto>>
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

        public async Task<IEnumerable<NotificationDto>> Handle(GetPersonalNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(_authenticatedUserService.UserId);

            var notifications = await _notificationRepository.GetRelevantOwnerNotificationsAsync(userId, 1, 1000);

            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }
    }
}