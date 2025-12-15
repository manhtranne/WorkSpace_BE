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
    public class GetSystemNotificationsQuery : IRequest<PagedResponse<IEnumerable<NotificationDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetSystemNotificationsQueryHandler : IRequestHandler<GetSystemNotificationsQuery, PagedResponse<IEnumerable<NotificationDto>>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public GetSystemNotificationsQueryHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<NotificationDto>>> Handle(GetSystemNotificationsQuery request, CancellationToken cancellationToken)
        {
            var validFilter = new RequestParameter
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            var notifications = await _notificationRepository.GetSystemNotificationsAsync(validFilter.PageNumber, validFilter.PageSize);
            var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

            return new PagedResponse<IEnumerable<NotificationDto>>(notificationDtos, validFilter.PageNumber, validFilter.PageSize);
        }
    }
}