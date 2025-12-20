using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Notifications_F.Queries
{
    public class GetNotificationsByWorkSpaceQuery : IRequest<Response<IEnumerable<Notification>>>
    {
        public int WorkSpaceId { get; set; } // Khách đang xem Workspace nào?
    }

    public class GetNotificationsByWorkSpaceQueryHandler : IRequestHandler<GetNotificationsByWorkSpaceQuery, Response<IEnumerable<Notification>>>
    {
        private readonly IApplicationDbContext _context;

        public GetNotificationsByWorkSpaceQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<Notification>>> Handle(GetNotificationsByWorkSpaceQuery request, CancellationToken cancellationToken)
        {
            // B1: Tìm Workspace để biết Owner (HostId) là ai
            var workspace = await _context.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.WorkSpaceId, cancellationToken);

            if (workspace == null)
            {
                throw new ApiException($"Workspace ID {request.WorkSpaceId} not found.");
            }

            // B2: Lấy tất cả thông báo được tạo bởi Owner đó (SenderId == HostId)
            var notifications = await _context.Set<Notification>()
                .Where(n => n.SenderId == workspace.HostId && n.SenderRole == "Owner")
                .OrderByDescending(n => n.CreateUtc)
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<Notification>>(notifications);
        }
    }
}