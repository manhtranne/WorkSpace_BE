// File: src/WorkSpace.Application/Features/Services/Queries/GetAllServicesGrouped/GetAllServicesGroupedQueryHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Interfaces; // Chứa IApplicationDbContext
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Services.Queries.GetAllServicesGrouped
{
    public class GetAllServicesGroupedQueryHandler : IRequestHandler<GetAllServicesGroupedQuery, Response<List<WorkSpaceWithServicesDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetAllServicesGroupedQueryHandler(
            IApplicationDbContext context,
            IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<List<WorkSpaceWithServicesDto>>> Handle(GetAllServicesGroupedQuery request, CancellationToken cancellationToken)
        {
            // 1. Lấy thông tin Host Profile dựa trên UserId của người đang đăng nhập
            // Bước này quan trọng để đảm bảo User thực sự là Owner
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);

            if (hostProfile == null)
            {
                // Nếu không tìm thấy Profile Owner, trả về list rỗng hoặc lỗi tùy bạn
                return new Response<List<WorkSpaceWithServicesDto>>(new List<WorkSpaceWithServicesDto>());
            }

            // 2. Truy vấn Workspace dựa trên HostId tìm được ở trên
            var workspaces = await _context.Workspaces
                .Include(w => w.Services) // Load bảng con Services
                .Where(w => w.HostId == hostProfile.Id) // Lọc theo HostId
                .Where(w => w.IsActive) // Chỉ lấy workspace đang hoạt động (tùy chọn)
                .Select(w => new WorkSpaceWithServicesDto
                {
                    WorkSpaceId = w.Id,
                    WorkSpaceTitle = w.Title,
                    Services = w.Services
                        .Where(s => s.IsActive) // Chỉ lấy service đang active
                        .Select(s => new WorkSpaceServiceDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description,
                            Price = s.Price,
                            ImageUrl = s.ImageUrl,
                            IsActive = s.IsActive,
                            WorkSpaceId = s.WorkSpaceId
                        }).ToList()
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new Response<List<WorkSpaceWithServicesDto>>(workspaces);
        }
    }
}