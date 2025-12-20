using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Reviews.Queries.GetReviewsByWorkSpace
{
    public class GetReviewsByWorkSpaceQuery : IRequest<Response<IEnumerable<WorkSpaceReviewDto>>>
    {
        public int WorkSpaceId { get; set; }
    }

    public class GetReviewsByWorkSpaceQueryHandler : IRequestHandler<GetReviewsByWorkSpaceQuery, Response<IEnumerable<WorkSpaceReviewDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetReviewsByWorkSpaceQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<WorkSpaceReviewDto>>> Handle(GetReviewsByWorkSpaceQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Reviews
                .Include(r => r.WorkSpaceRoom)
                    .ThenInclude(room => room.WorkSpaceRoomType)
                .Include(r => r.User)
                .Where(r => r.WorkSpaceRoom.WorkSpaceId == request.WorkSpaceId &&
                            r.IsVerified == true &&
                            r.IsPublic == true)
                .AsNoTracking();

            query = query.OrderByDescending(r => r.CreateUtc);

            var reviews = await query.Select(r => new WorkSpaceReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedDate = r.CreateUtc,

                UserId = r.UserId,
                ReviewerName = (r.User.FirstName + " " + r.User.LastName).Trim() == "" ? r.User.UserName : (r.User.FirstName + " " + r.User.LastName).Trim(),
                ReviewerAvatar = r.User.Avatar,

                RoomId = r.WorkSpaceRoomId,
                RoomName = r.WorkSpaceRoom.Title,
                RoomType = r.WorkSpaceRoom.WorkSpaceRoomType != null ? r.WorkSpaceRoom.WorkSpaceRoomType.Name : ""
            }).ToListAsync(cancellationToken);

            return new Response<IEnumerable<WorkSpaceReviewDto>>(reviews);
        }
    }
}