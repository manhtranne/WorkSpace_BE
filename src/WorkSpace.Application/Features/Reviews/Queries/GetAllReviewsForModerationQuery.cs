using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkSpace.Application.Features.Reviews.Queries;

public class GetAllReviewsForModerationQuery : IRequest<IEnumerable<ReviewModerationDto>>
{
    public bool? IsVerifiedFilter { get; set; }
    public bool? IsPublicFilter { get; set; }
}

public class GetAllReviewsForModerationQueryHandler : IRequestHandler<GetAllReviewsForModerationQuery, IEnumerable<ReviewModerationDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllReviewsForModerationQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ReviewModerationDto>> Handle(GetAllReviewsForModerationQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.WorkSpaceRoom.WorkSpace) // Include thêm WorkSpace để lấy thông tin đầy đủ nếu cần
            .AsNoTracking();

        // --- LOGIC MỚI ---
        // Nếu có truyền filter cụ thể (true/false) -> Lọc theo yêu cầu
        // Nếu KHÔNG truyền (null) -> Mặc định chỉ lấy những cái CHƯA DUYỆT (false)
        if (request.IsVerifiedFilter.HasValue)
        {
            query = query.Where(r => r.IsVerified == request.IsVerifiedFilter.Value);
        }
        else
        {
            // Mặc định: Chỉ hiện review chưa duyệt (Pending)
            query = query.Where(r => r.IsVerified == false);
        }

        if (request.IsPublicFilter.HasValue)
        {
            query = query.Where(r => r.IsPublic == request.IsPublicFilter.Value);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreateUtc)
            .ToListAsync(cancellationToken);

        var reviewDtos = reviews.Select(r => new ReviewModerationDto
        {
            Id = r.Id,
            BookingId = r.BookingId,
            UserId = r.UserId,
            UserName = r.User?.GetFullName(),
            WorkSpaceRoomId = r.WorkSpaceRoomId,
            WorkSpaceRoomTitle = r.WorkSpaceRoom?.Title, 
            Rating = r.Rating,
            Comment = r.Comment,
            IsVerified = r.IsVerified,
            IsPublic = r.IsPublic,
            CreateUtc = r.CreateUtc
        }).ToList();

        return reviewDtos;
    }
}