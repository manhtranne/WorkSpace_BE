using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Services;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Services.Queries.GetServicesByWorkSpace
{
    public class GetServicesByWorkSpaceQuery : IRequest<Response<List<WorkSpaceServiceDto>>>
    {
        public int WorkSpaceId { get; set; }
        public int OwnerUserId { get; set; } // Nếu muốn check quyền xem, hoặc bỏ qua nếu public xem được
    }

    public class GetServicesByWorkSpaceQueryHandler : IRequestHandler<GetServicesByWorkSpaceQuery, Response<List<WorkSpaceServiceDto>>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceService> _serviceRepository;
        private readonly IMapper _mapper;

        public GetServicesByWorkSpaceQueryHandler(IGenericRepositoryAsync<WorkSpaceService> serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<WorkSpaceServiceDto>>> Handle(GetServicesByWorkSpaceQuery request, CancellationToken cancellationToken)
        {
            // Logic lấy service, có thể filter IsActive = true nếu muốn chỉ hiện cái đang hoạt động
            // Giả sử lấy tất cả của workspace đó
            var allServices = await _serviceRepository.GetPagedResponseAsync(1, 100); // Hoặc viết hàm GetWhere trong Repo

            // Cách đơn giản nhất nếu dùng GenericRepo chưa có hàm GetByCondition:
            // Bạn nên implement thêm hàm GetListByWorkSpaceId trong ServiceRepository hoặc dùng IQueryable

            // Tạm dùng logic giả định lấy tất cả và lọc client-side (Not recommended for prod)
            // Khuyến nghị: Inject IWorkSpaceServiceRepository và viết hàm GetByWorkSpaceId(int id)

            var services = (await _serviceRepository.GetAllAsync())
                            .Where(x => x.WorkSpaceId == request.WorkSpaceId && x.IsActive)
                            .ToList();

            var dtos = _mapper.Map<List<WorkSpaceServiceDto>>(services);
            return new Response<List<WorkSpaceServiceDto>>(dtos);
        }
    }
}