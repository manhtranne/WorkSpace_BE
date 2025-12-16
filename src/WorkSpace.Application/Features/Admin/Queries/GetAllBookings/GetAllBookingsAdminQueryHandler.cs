using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Features.Admin.Queries.GetAllBookings;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Admin.Queries.GetAllBookings
{
    public class GetAllBookingsAdminQueryHandler : IRequestHandler<GetAllBookingsAdminQuery, PagedResponse<IEnumerable<BookingAdminDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllBookingsAdminQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<BookingAdminDto>>> Handle(GetAllBookingsAdminQuery request, CancellationToken cancellationToken)
        {
           
            var query = _context.Bookings
                .Include(b => b.Customer)                    
                .Include(b => b.BookingStatus)
                .Include(b => b.WorkSpaceRoom)                
                    .ThenInclude(r => r.WorkSpace)            
                .AsNoTracking()
                .AsQueryable();

       
            if (request.StatusId.HasValue)
            {
                query = query.Where(b => b.BookingStatusId == request.StatusId.Value);
            }

      
            if (request.WorkspaceId.HasValue)
            {
                query = query.Where(b => b.WorkSpaceRoom.WorkSpaceId == request.WorkspaceId.Value);
            }

         
            if (request.FromDate.HasValue)
            {
                query = query.Where(b => b.StartTimeUtc >= request.FromDate.Value);
            }
            if (request.ToDate.HasValue)
            {
                query = query.Where(b => b.StartTimeUtc <= request.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();

                query = query.Where(b =>
                    b.BookingCode.ToLower().Contains(term) ||                        
                    (b.Customer != null && (                                         
                        b.Customer.UserName.ToLower().Contains(term) ||
                        b.Customer.Email.ToLower().Contains(term) ||
                        b.Customer.FirstName.ToLower().Contains(term) ||
                        b.Customer.LastName.ToLower().Contains(term)
                    )) ||
                    b.WorkSpaceRoom.Title.ToLower().Contains(term) ||                
                    b.WorkSpaceRoom.WorkSpace.Title.ToLower().Contains(term)         
                );
            }

            query = query.OrderByDescending(b => b.StartTimeUtc); 

            var totalRecords = await query.CountAsync(cancellationToken);

            var bookings = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var bookingDtos = _mapper.Map<IEnumerable<BookingAdminDto>>(bookings);

            return new PagedResponse<IEnumerable<BookingAdminDto>>(bookingDtos, request.PageNumber, request.PageSize, totalRecords);
        }
    }
}