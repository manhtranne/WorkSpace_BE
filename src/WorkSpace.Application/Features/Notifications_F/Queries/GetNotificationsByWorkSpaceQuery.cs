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
        public int WorkSpaceId { get; set; }
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
            // 1. Tìm Workspace và Include luôn HostProfile để lấy được UserId gốc
            var workspace = await _context.Workspaces
                .Include(w => w.Host) // [QUAN TRỌNG] Include Host để truy cập UserId
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.WorkSpaceId, cancellationToken);

            if (workspace == null)
            {
                throw new ApiException($"Workspace ID {request.WorkSpaceId} not found.");
            }

            if (workspace.Host == null)
            {
                return new Response<IEnumerable<Notification>>(new List<Notification>(), "Workspace chưa có Owner/Host hợp lệ.");
            }

            // 2. Lấy UserId thật của Owner từ HostProfile
            var ownerUserId = workspace.Host.UserId;

            // 3. Tìm thông báo có SenderId trùng với UserId của Owner
            var notifications = await _context.Set<Notification>()
                .Where(n => n.SenderId == ownerUserId && n.SenderRole == "Owner")
                .OrderByDescending(n => n.CreateUtc)
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<Notification>>(notifications);
        }
    }
}