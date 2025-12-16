using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Parameters;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Notifications.Queries
{
    public class GetSystemNotificationsQuery : IRequest<IEnumerable<NotificationDto>>
    {

    }
    public class GetSystemNotificationsQueryHandler : IRequestHandler<GetSystemNotificationsQuery, IEnumerable<NotificationDto>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public GetSystemNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotificationDto>> Handle(GetSystemNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _notificationRepository.GetSystemNotificationsAsync(1, 1000);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }
    }
}