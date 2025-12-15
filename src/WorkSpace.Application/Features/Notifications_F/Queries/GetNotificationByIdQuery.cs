using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Notifications_F.Queries
{
    public class GetNotificationByIdQuery : IRequest<Notification>
    {
        public int Id { get; set; }
    }

    public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Notification>
    {
        private readonly INotificationRepository _notificationRepository;

        public GetNotificationByIdQueryHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Notification> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.Id);
            if (notification == null)
                throw new ApiException($"Không tìm thấy thông báo với ID {request.Id}");

            return notification;
        }
    }
}