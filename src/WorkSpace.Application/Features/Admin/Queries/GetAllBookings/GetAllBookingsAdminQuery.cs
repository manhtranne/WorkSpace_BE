using MediatR;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Parameters;
using System.Collections.Generic;
using System;

namespace WorkSpace.Application.Features.Admin.Queries.GetAllBookings
{
    public class GetAllBookingsAdminQuery : IRequest<PagedResponse<IEnumerable<BookingAdminDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SearchTerm { get; set; } 
        public int? StatusId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? WorkspaceId { get; set; }
    }
}